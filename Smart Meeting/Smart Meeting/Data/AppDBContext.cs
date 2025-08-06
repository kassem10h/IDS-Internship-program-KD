using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Models;

namespace Smart_Meeting.Data
{
    public class AppDBContext : IdentityDbContext<Employee>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        //DbSets
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<RoomFeatures> RoomFeatures { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MinutesOfMeeting> MinutesOfMeetings { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base method to apply default Identity configurations
            base.OnModelCreating(modelBuilder);

            // Define a list of roles to seed into the database
            List<IdentityRole> roles = new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Name = "Admin",   // Display name of the role
                    NormalizedName = "ADMIN"   // Uppercase version used for comparisons
                },
                 new IdentityRole
                {
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE"
                },
                  new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                },
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);// Use Entity Framework to seed the roles into the IdentityRole table



            //Attendee: Many-to-Many between Employee and Meeting
            modelBuilder.Entity<Attendee>()
                .HasKey(a => new { a.EmployeeID, a.MeetingID });

            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.employee)
                .WithMany(e => e.Attendees)
                .HasForeignKey(a => a.EmployeeID);

            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.meeting)
                .WithMany(m => m.Attendees)
                .HasForeignKey(a => a.MeetingID)
                .OnDelete(DeleteBehavior.Restrict); ;

            // MinutesOfMeeting: Many-to-One with Employee (Author)
            modelBuilder.Entity<MinutesOfMeeting>()
                .HasOne(m => m.Author)
                .WithMany(e => e.AuthoredMinutes)
                .HasForeignKey(m => m.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // MinutesOfMeeting: One-to-One with Meeting
            modelBuilder.Entity<MinutesOfMeeting>()
                .HasOne(m => m.Meeting)
                .WithOne(meeting => meeting.MinutesOfMeeting)
                .HasForeignKey<MinutesOfMeeting>(m => m.MeetingID);
        }
    }
}