using Microsoft.AspNetCore.Authorization;

namespace SmartMeeting.Authentication.AuthModel
{
    public class CustomPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public CustomPermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class CustomPermissionHandler : AuthorizationHandler<CustomPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomPermissionRequirement requirement)
        {
            // Check if user has the required permission
            var userPermissions = context.User.FindAll("permission").Select(c => c.Value);

            if (userPermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
