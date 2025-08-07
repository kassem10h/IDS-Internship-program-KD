namespace SmartMeeting.Models
{
    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string User = "User";

        public static readonly string[] AllRoles = { Admin, User };

        public static class Policies
        {
            public const string AdminOnly = "AdminOnly";
            public const string UserOrAdmin = "UserOrAdmin";
        }
    }
}
