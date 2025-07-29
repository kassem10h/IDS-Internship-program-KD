using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Models;


namespace Smart_Meeting.Data
{
    public class AppDBContext:DbContext
    {
        public AppDBContext(DbContextOptions dbContextOptions) 
            : base(dbContextOptions) { }
        //constructor accepts DbContextOptions object that has
        //configurations(server,connection) and its passed to the base class DBContext

        public DbSet<Room> Rooms { get; set; } //Room is the model name, Rooms is the database table name
    }
}
