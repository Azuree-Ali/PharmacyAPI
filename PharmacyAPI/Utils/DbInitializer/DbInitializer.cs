using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyAPI.DataAccess;
using PharmacyAPI.Models;
using PharmacyAPI.Utils;
using PharmacyAPI.Utils.DbInitializer;

namespace PharmacyAPI.Utils.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<DbInitializer> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                // Roles
                if (!await _roleManager.RoleExistsAsync(CD.SUPER_ADMIN_ROLE))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(CD.SUPER_ADMIN_ROLE));
                }

                if (!await _roleManager.RoleExistsAsync(CD.ADMIN_ROLE))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(CD.ADMIN_ROLE));
                }

                if (!await _roleManager.RoleExistsAsync(CD.PHARMACIST_ROLE))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(CD.PHARMACIST_ROLE));
                }

                if (!await _roleManager.RoleExistsAsync(CD.CUSTOMER_ROLE))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole(CD.CUSTOMER_ROLE));
                }

                // Super Admin
                var superAdmin = await _userManager.FindByEmailAsync(
                    "superadmin@eraasoft.com");

                if (superAdmin == null)
                {
                    superAdmin = new ApplicationUser()
                    {
                        FirstName = "Super",
                        LastName = "Admin",
                        UserName = "SuperAdmin",
                        Email = "superadmin@eraasoft.com",
                        EmailConfirmed = true,
                    };

                    var result = await _userManager.CreateAsync(
                        superAdmin,
                        "SuperAdmin@123");

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(
                            superAdmin,
                            CD.SUPER_ADMIN_ROLE);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while initializing database.");
            }


        }
    }
}
