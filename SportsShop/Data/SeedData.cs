using Microsoft.AspNetCore.Identity;
using SportsShop.Models;

namespace SportsShop.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Tạo roles
            string[] roleNames = { "Admin", "Customer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo Admin user
            var adminEmail = "admin@sportsshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Tạo Customer user mẫu
            var customerEmail = "customer@gmail.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);

            if (customerUser == null)
            {
                var newCustomer = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Nguyễn Văn A",
                    PhoneNumber = "0987654321",
                    Address = "Hồ Chí Minh",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newCustomer, "Customer@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newCustomer, "Customer");
                }
            }
        }
    }
}