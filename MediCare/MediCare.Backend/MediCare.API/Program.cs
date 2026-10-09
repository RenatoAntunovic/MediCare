using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Market.API;
using Market.API.Middlewares;
using Market.Application;
using Market.Infrastructure;
using MediCare.API.FCM;
using MediCare.Application.Abstractions;
using MediCare.Application.Common.Behaviors;
using MediCare.Application.Modules.FCM.Services;
using MediCare.Application.Modules.MedicineSearch;
using MediCare.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Microsoft.AspNetCore.StaticFiles; 


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

                options.AddFixedWindowLimiter("default", limiter =>
                {
                    limiter.PermitLimit = 100;
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("login", limiter =>
                {
                    limiter.PermitLimit = 5;
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("search", limiter =>
                {
                    limiter.PermitLimit = 2;
                    limiter.Window = TimeSpan.FromSeconds(5);
                    limiter.QueueLimit = 0;
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddSingleton<IFcmService, FcmService>();
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
