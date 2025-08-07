using Microsoft.AspNetCore.Identity;
using SmartMeeting.Authentication.AuthModel;
using SmartMeeting.Data;
using SmartMeeting.Models;

namespace SmartMeeting.Configurations
{
    public static class IdentityConfiguration
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            // Register authorization handlers
            services.AddScoped<AdminOnlyHandler>();
            services.AddScoped<UserOrAdminHandler>();
            services.AddScoped<ResourceOwnerHandler>();
            services.AddScoped<CustomPermissionHandler>();

            services.AddAuthorization(options =>
            {
                // Role-based policies
                options.AddPolicy(RoleConstants.Policies.AdminOnly, policy =>
                    policy.RequireRole(RoleConstants.Admin));

                options.AddPolicy(RoleConstants.Policies.UserOrAdmin, policy =>
                    policy.RequireRole(RoleConstants.User, RoleConstants.Admin));

                // Custom requirement policies
                options.AddPolicy("AdminOnlyPolicy", policy =>
                    policy.Requirements.Add(new AdminOnlyRequirement()));

                options.AddPolicy("UserOrAdminPolicy", policy =>
                    policy.Requirements.Add(new UserOrAdminRequirement()));

                options.AddPolicy("ResourceOwnerPolicy", policy =>
                    policy.Requirements.Add(new ResourceOwnerRequirement()));

                // Custom permission policy example
                options.AddPolicy("ManageUsersPermission", policy =>
                    policy.Requirements.Add(new CustomPermissionRequirement("manage_users")));
            });

            return services;
        }
    }
}
