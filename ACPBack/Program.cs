
using Aspose.Cells.Charts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using stage_api.configuration;
using stage_api.Models;
using System.Configuration;
using System.Text;

namespace stage_api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{

			var builder = WebApplication.CreateBuilder(args);

            var startup = new Startup(builder.Configuration);

            startup.ConfigureServices(builder.Services);

            // Add services to the container.
            builder.Services.AddControllers();
			//builder.Services.AddScoped<AuthenticationService>();
			builder.Services.AddScoped<DataProcessingService>();
			builder.Services.AddScoped<FileUploadService>();

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial

			var app = builder.Build();

            startup.Configure(app, app.Lifetime);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

            app.UseCors(builder =>
            builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthorization();
			app.UseCors("AllowSpecificOrigin");
			app.MapControllers();

			app.MapGet("/DataBaseConnection", () => "Database connection established successfully.");

            using(var scope = app.Services.CreateScope())
            {
                // Creating roles if they do not exist
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "ADMIN", "USER" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Creating the admin account if it does not exist
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var adminEmail = "admin@admin.com";
                var adminUserName = "admin";
                var password = "Azerty1234_";

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var user = new ApplicationUser();
                    user.UserName = adminUserName;
                    user.Email = adminEmail;
                    user.FullName = "Application Administrator";

                    await userManager.CreateAsync(user, password);
                    await userManager.AddToRoleAsync(user, "ADMIN");
                }
            }

			app.Run();
		}
	}
}
