using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartMeeting.Configurations;
using SmartMeeting.Data;
using SmartMeeting.Filters;
using SmartMeeting.Helpers;
using SmartMeeting.Middleware;
using SmartMeeting.Repositories.Implementations;
using SmartMeeting.Repositories.Interfaces;
using SmartMeeting.Services.Implementations;
using SmartMeeting.Services.Interfaces;
using SmartMeeting.Settings;
using System.Text;

namespace SmartMeeting
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            await ConfigurePipelineAsync(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            });

            services.AddEndpointsApiExplorer();

            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DevConnection")));

            // Identity & Authorization (Make sure AddIdentityConfiguration registers Identity using ApplicationDbContext)
            services.AddIdentityConfiguration();
            services.AddAuthorizationPolicies();

            // JWT settings binding
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

            // Debug: print loaded JWT config (remove in production)
            Console.WriteLine($"JWT Issuer: '{jwtSettings.Issuer}'");
            Console.WriteLine($"JWT Audience: '{jwtSettings.Audience}'");
            Console.WriteLine($"JWT Secret length: '{(jwtSettings.SecretKey ?? string.Empty).Length}'");

            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            {
                // Throwing here will stop app startup so you don't proceed in broken state
                throw new InvalidOperationException("JWT SecretKey is missing. Set 'JWT:SecretKey' in appsettings.json or env variables.");
            }

            services.AddSingleton(jwtSettings);

            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Swagger (with security)
            services.AddSwaggerSecurity();

            services.AddAutoMapper(typeof(Program));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p => p
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            // DI
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<JwtTokenGenerator>();
            services.AddScoped<TokenValidationHelper>();

            // Health checks
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            services.AddLogging();
        }

        private static async Task ConfigurePipelineAsync(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartMeeting API V1");
                    c.RoutePrefix = string.Empty;
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    c.DefaultModelsExpandDepth(-1);
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            // Error handling should be early
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();

            // Seed DB
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    await ApplicationDbContext.InitializeAsync(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while initializing the database");
                }
            }
        }
    }
}
