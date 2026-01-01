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
using MediCare.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;


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

            //
            // 1) Standard builder (includes appsettings.json, appsettings.{ENV}.json,
            //    environment variables, user-secrets (Dev), and command-line args)
            //
            var builder = WebApplication.CreateBuilder(args);

            // 2) Promote Serilog to full configuration from builder.Configuration
            //    (reads "Serilog" section from appsettings + ENV overrides)
            //
            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)   // Serilog section in appsettings
                   .ReadFrom.Services(services)                 // DI enrichers if any
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.WithProcessId()
                   .Enrich.WithMachineName();
            });

            // Optional: remove default providers to have only Serilog
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

                // Default limiter (npr za većinu API-ja)
                options.AddFixedWindowLimiter("default", limiter =>
                {
                    limiter.PermitLimit = 100; // 100 requesta
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueLimit = 0;
                });

                // Login limiter (strožiji)
                options.AddFixedWindowLimiter("login", limiter =>
                {
                    limiter.PermitLimit = 5; // 5 pokušaja
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueLimit = 0;
                });

                // Search limiter (strogi)
                options.AddFixedWindowLimiter("search", limiter =>
                {
                    limiter.PermitLimit = 2;  // Max 2 requesta
                    limiter.Window = TimeSpan.FromSeconds(5); // U 5 sekundi
                    limiter.QueueLimit = 0;
                });
            });

            //Kaze sejtan da pomocu ovog ispisuje Ime mora biti puno i to gresku baci ali ne radi moraju se skinuti ovi
            //paketi tako da moramo skontati nesto drugo

            //builder.Services.AddControllers()
            //.AddFluentValidation(fv =>
            //{
            //    fv.RegisterValidatorsFromAssemblyContaining<UpdateMedicineCommandValidator>();
            //    fv.DisableDataAnnotationsValidation = true;
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
            });

            // Registracija pipeline behavior za MediatR i FluentValidation
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddSingleton<IFcmService, FcmService>();
            builder.Services.AddHttpClient(); // HttpClient za FcmService

            // Registracija FcmService

            // ===== ELASTICSEARCH CONFIGURATION =====
            var esUri = builder.Configuration["Elasticsearch:Uri"];
            var esUsername = builder.Configuration["Elasticsearch:Username"];
            var esPassword = builder.Configuration["Elasticsearch:Password"];

            var settings = new ElasticsearchClientSettings(new Uri(esUri))
                .Authentication(new BasicAuthentication(esUsername, esPassword))
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true);

            var elasticClient = new ElasticsearchClient(settings);
            builder.Services.AddSingleton(elasticClient);

            // ===== REGISTER ELASTICSEARCH SERVICE =====
            builder.Services.AddSingleton<ElasticsearchService>();


            var app = builder.Build();

            // ---------------------------------------------------------
            // 4. Middleware pipeline
            // ---------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Global exception handler (IExceptionHandler)
            app.UseExceptionHandler();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images")
                ),
                RequestPath = "/images",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    // dozvoli svima da vide fajlove
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                }
            });
            app.UseHttpsRedirection();
            app.UseCors("AllowAngularDev");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapControllers();

            // Database migrations + seeding
            await app.Services.InitializeDatabaseAsync(app.Environment);

            // ===== ELASTICSEARCH INDEX INITIALIZATION =====
            var elasticsearchService = app.Services.GetRequiredService<ElasticsearchService>();

            // Obriši stari index
            await elasticsearchService.DeleteProductIndexAsync();

            // Kreiraj novi sa novim mappingom
            await elasticsearchService.CreateProductIndexAsync();

            // ===== SYNC MEDICINES FROM DATABASE TO ELASTICSEARCH =====
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                await elasticsearchService.SyncMedicinesFromDatabaseAsync(dbContext);
                Log.Information("Medicines synced to Elasticsearch.");
            }

            Log.Information("MediCare API started successfully.");
            app.Run();
        }

        catch (HostAbortedException)
        {
            // EF Core tools abortiraju host nakon što uzmu DbContext.
            // Ovo nije runtime greška – samo tiho izađi.
            Log.Information("Host aborted by EF Core tooling (design-time) - its ok.");
        }
        catch (Exception ex)
        {
            // Any startup failure will be logged here
            Log.Fatal(ex, "MediCare API terminated unexpectedly.");
        }
        finally
        {
            // Ensure all logs are flushed before the app exits
            Log.CloseAndFlush();
        }
    }
}
