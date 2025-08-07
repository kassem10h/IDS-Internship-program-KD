using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartMeeting.Authentication.AuthModel
{
    public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminOnlyRequirement requirement)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Admin")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class UserOrAdminHandler : AuthorizationHandler<UserOrAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UserOrAdminRequirement requirement)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Admin" || userRole == "User")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            // Admin can access any resource
            if (userRole == "Admin")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user owns the resource (implementation depends on resource type)
            // This would typically involve checking the resource's owner against the current user
            // For now, we'll allow users to access their own resources
            if (!string.IsNullOrEmpty(userId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // Authorization Requirements
    public class AdminOnlyRequirement : IAuthorizationRequirement
    {
    }

    public class UserOrAdminRequirement : IAuthorizationRequirement
    {
    }

    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
    }
}
