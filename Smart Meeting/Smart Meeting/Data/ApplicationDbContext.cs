using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Models;
using SmartMeeting.Models;

namespace SmartMeeting.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Authentication entities
        public DbSet<RefreshToken> RefreshTokens { get; set; }
         
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomFeatures> RoomFeatures { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MinutesOfMeeting> MinutesOfMeetings { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
             
            List<IdentityRole> roles = new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE"
                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "User",
                    NormalizedName = "USER"
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);

            // Configure ApplicationUser (new authentication user)
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedByIp).HasMaxLength(50);
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
            });
             

            // Attendee: Many-to-Many between ApplicationUser and Meeting
            builder.Entity<Attendee>()
                .HasKey(a => new { a.EmployeeID, a.MeetingID });

            builder.Entity<Attendee>()
                .HasOne(a => a.employee)
                .WithMany(e => e.Attendees)
                .HasForeignKey(a => a.EmployeeID);

            builder.Entity<Attendee>()
                .HasOne(a => a.meeting)
                .WithMany(m => m.Attendees)
                .HasForeignKey(a => a.MeetingID)
                .OnDelete(DeleteBehavior.Restrict);

            // MinutesOfMeeting: Many-to-One with ApplicationUser (Author)
            builder.Entity<MinutesOfMeeting>()
                .HasOne(m => m.Author)
                .WithMany(e => e.AuthoredMinutes)
                .HasForeignKey(m => m.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // MinutesOfMeeting: One-to-One with Meeting
            builder.Entity<MinutesOfMeeting>()
                .HasOne(m => m.Meeting)
                .WithOne(meeting => meeting.MinutesOfMeeting)
                .HasForeignKey<MinutesOfMeeting>(m => m.MeetingID);

            // Room and RoomFeatures relationship
            builder.Entity<RoomFeatures>()
                .HasOne(rf => rf.Room)
                .WithOne(r => r.RoomFeatures)
                .HasForeignKey<RoomFeatures>(rf => rf.RoomID);
        }

        // Database Initialization Methods
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed admin user
            await SeedAdminUserAsync(userManager);
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@smartmeeting.com";
            const string adminPassword = "Admin123!@#";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        // Helper methods
        public async Task CleanupExpiredRefreshTokensAsync()
        {
            var expiredTokens = RefreshTokens.Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked);
            RefreshTokens.RemoveRange(expiredTokens);
            await SaveChangesAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await Users.CountAsync(u => u.IsActive);
        }
    }
}
