using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PharmacyAPI.DataAccess;
using PharmacyAPI.Models;
using PharmacyAPI.Utils.DbInitializer;
using Scalar.AspNetCore;
using System.Text;

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

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                     {
                          ValidateIssuer = true,
                           ValidateAudience = true,
                          ValidateLifetime = true,
                           ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtSettings["ValidateIssuer"],
                             ValidAudience = jwtSettings["ValidateAudience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
                     };
             });

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