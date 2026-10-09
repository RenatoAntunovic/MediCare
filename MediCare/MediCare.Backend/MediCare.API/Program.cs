using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Market.API;
using Market.API.Middlewares;
using Market.Application;
using Market.Infrastructure;
using MediCare.Application.Abstractions;
using MediCare.Application.Common.Behaviors;
using MediCare.Application.Modules.MedicineSearch;
using MediCare.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Microsoft.AspNetCore.StaticFiles;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediCare.API.Notifications;
using System.Security.Claims;
using System.Threading.RateLimiting;


public partial class Program
{
    private static async Task Main(string[] args)
    {
        //
        // 0) Bootstrap logger (very early, no full config yet)
        //
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // minimal sink so we see startup errors
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting MediCare API...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)
                   .ReadFrom.Services(services)
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.WithProcessId()
                   .Enrich.WithMachineName();
            });

            builder.Logging.ClearProviders();

            // ---------------------------------------------------------
            // 3. Layer registrations
            // ---------------------------------------------------------
            builder.Services
                .AddAPI(builder.Configuration, builder.Environment)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddApplication();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Every limit is counted PER CLIENT (logged-in user, otherwise IP address),
                // so one client hitting the limit never blocks everybody else.
                static string ClientKey(HttpContext ctx) =>
                    ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? ctx.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";

                static RateLimitPartition<string> PerClient(HttpContext ctx, int permitLimit, TimeSpan window) =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0
                    });

                // Global limit for the whole API: 200 requests per minute per client
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

                // Login: 5 attempts per minute per IP address (brute-force protection)
                options.AddPolicy("login", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                // Search: 10 requests per 10 seconds per client
                options.AddPolicy("search", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ClientKey(ctx), _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(10),
                        QueueLimit = 0
                    }));

                // Register: 3 new accounts per 10 minutes per IP address (fake account spam)
                options.AddPolicy("register", ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0
                        }));

                // Checkout and reservations: 5 per minute per user (order / booking spam)
                options.AddPolicy("orders", ctx => PerClient(ctx, 5, TimeSpan.FromMinutes(1)));

                // PDF generation is expensive: 10 per minute per user
                options.AddPolicy("reports", ctx => PerClient(ctx, 10, TimeSpan.FromMinutes(1)));

                // Test notifications: 5 per minute per user
                options.AddPolicy("notifications", ctx => PerClient(ctx, 5, TimeSpan.FromMinutes(1)));

                // Tell the client how long to wait and return a readable message
                options.OnRejected = async (context, ct) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        code = "rate.limit",
                        message = "Previše zahtjeva. Sačekajte malo pa pokušajte ponovo."
                    }, ct);
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
                              .WithExposedHeaders("Retry-After"); // lets the frontend read how long to wait
                    });
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // ---------------------------------------------------------
            // PUSH NOTIFICATIONS (Firebase Cloud Messaging)
            // The key file is NOT in git. Without it the app runs normally, notifications are skipped.
            // ---------------------------------------------------------
            var firebaseKeyPath = Path.Combine(
                builder.Environment.ContentRootPath,
                builder.Configuration["Firebase:CredentialsPath"] ?? "firebase-adminsdk.json");

            if (File.Exists(firebaseKeyPath))
            {
                FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(firebaseKeyPath) });
                builder.Services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();
                Log.Information("Firebase push notifications enabled.");
            }
            else
            {
                builder.Services.AddScoped<IPushNotificationService, NoOpPushNotificationService>();
                Log.Warning("firebase-adminsdk.json not found – push notifications are disabled.");
            }

            builder.Services.AddHttpClient();

            // ---------------------------------------------------------
            // FULL-TEXT SEARCH – Search:Provider in appsettings.json
            //   "Lucene"        → Lucene.Net, runs inside the API (default, no extra server needed)
            //   "Sql"           → plain SQL LIKE search (always works, simplest)
            //   "Elasticsearch" → external Elasticsearch server (must be running)
            // For backward compatibility, Elasticsearch:Enabled = true still selects Elasticsearch.
            // ---------------------------------------------------------
            var searchProvider = (builder.Configuration["Search:Provider"]
                    ?? (builder.Configuration.GetValue<bool>("Elasticsearch:Enabled") ? "Elasticsearch" : "Lucene"))
                .Trim()
                .ToLowerInvariant();

            switch (searchProvider)
            {
                case "lucene":
                    builder.Services.AddSingleton<LuceneMedicineSearchService>();
                    builder.Services.AddSingleton<IMedicineSearchService>(sp => sp.GetRequiredService<LuceneMedicineSearchService>());
                    builder.Services.AddSingleton<IMedicineSearchIndex>(sp => sp.GetRequiredService<LuceneMedicineSearchService>());
                    break;

                case "sql":
                    builder.Services.AddScoped<IMedicineSearchService, SqlMedicineSearchService>();
                    builder.Services.AddSingleton<IMedicineSearchIndex, NoOpMedicineSearchIndex>();
                    break;

                case "elasticsearch":
                {
                    var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "https://localhost:9200";
                    var esUsername = builder.Configuration["Elasticsearch:Username"] ?? "elastic";
                    var esPassword = builder.Configuration["Elasticsearch:Password"] ?? string.Empty;
                    var settings = new ElasticsearchClientSettings(new Uri(esUri))
                        .Authentication(new BasicAuthentication(esUsername, esPassword))
                        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true); // local development only
                    builder.Services.AddSingleton(new ElasticsearchClient(settings));
                    builder.Services.AddSingleton<ElasticsearchService>();
                    builder.Services.AddSingleton<IMedicineSearchService>(sp => sp.GetRequiredService<ElasticsearchService>());
                    builder.Services.AddSingleton<IMedicineSearchIndex>(sp => sp.GetRequiredService<ElasticsearchService>());
                    break;
                }

                default:
                    throw new InvalidOperationException(
                        $"Unknown Search:Provider '{searchProvider}'. Allowed values: Lucene, Sql, Elasticsearch.");
            }

            var app = builder.Build();

            // ---------------------------------------------------------
            // 4. Middleware pipeline
            // ---------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseStaticFiles();

            var imageContentTypes = new FileExtensionContentTypeProvider();
            imageContentTypes.Mappings[".jfif"] = "image/jpeg";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images")
                ),
                RequestPath = "/images",
                ContentTypeProvider = imageContentTypes,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }
            });
            app.UseCors("AllowAngularDev");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapControllers();

            // Database migrations + seeding
            await app.Services.InitializeDatabaseAsync(app.Environment);

            // ===== SEARCH INDEX: build it from the database on startup =====
            // Lucene keeps its index in memory, so it must be rebuilt on every start.
            // If the index cannot be built (e.g. Elasticsearch is down), the application still starts.
            if (searchProvider is "lucene" or "elasticsearch")
            {
                try
                {
                    await using var scope = app.Services.CreateAsyncScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    var count = await sender.Send(new RebuildMedicineSearchIndexCommand());
                    Log.Information("Search index ({Provider}) built: {Count} medicines.", searchProvider, count);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Search index ({Provider}) could not be built on startup. Search will not work until POST /api/sync/sync-medicines succeeds.", searchProvider);
                }
            }

            Log.Information("MediCare API started successfully.");
            app.Run();
        }

        catch (HostAbortedException)
        {
            Log.Information("Host aborted by EF Core tooling (design-time) - its ok.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "MediCare API terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
