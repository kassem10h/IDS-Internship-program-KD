
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Data;

namespace Smart_Meeting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDBContext>(options =>
              {options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); 
             }); //registers our custom DBContextand specifies the we are using sqlserver
                 //as db provider using the connection string retrieved from appsettings.json file

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
