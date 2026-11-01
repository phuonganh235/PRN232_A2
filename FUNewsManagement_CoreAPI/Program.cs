using FUNewsManagement_CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace FUNewsManagement_CoreAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext: mỗi request sẽ được cấp một "người phiên dịch" riêng
            builder.Services.AddDbContext<FUNewsManagementContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();   // cho phép truy cập file trong wwwroot qua URL

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
