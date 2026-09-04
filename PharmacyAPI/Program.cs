using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyAPI.DataAccess;
using PharmacyAPI.Models;
using PharmacyAPI.Utils.DbInitializer;
using Scalar.AspNetCore;

namespace PharmacyAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // Services
            // =========================

            builder.Services.AddControllers();

            // OpenAPI
            builder.Services.AddOpenApi();

            // Authentication
            builder.Services
                .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            // Authorization
            builder.Services.AddAuthorization();

            // Database
            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Custom Services
            builder.Services.ConfigureServices();

            var app = builder.Build();

            // =========================
            // Database Initializer
            // =========================

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider
                    .GetRequiredService<IDbInitializer>();

                await dbInitializer.InitializeAsync();
            }

            // =========================
            // HTTP Request Pipeline
            // =========================

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}