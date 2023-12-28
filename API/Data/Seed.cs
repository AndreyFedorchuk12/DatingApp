using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task ClearConnection(DataContext dataContext)
    {
        dataContext.Connections.RemoveRange(dataContext.Connections);
        await dataContext.SaveChangesAsync();
    }
    public static async Task SeedUser(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if(await userManager.Users.AnyAsync())
            return;
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        var roles = new List<AppRole>
        {
            new() { Name = "Member" },
            new() { Name = "Admin" },
            new() { Name = "Moderator" }
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }
        
        if (users != null)
        {
            foreach (var user in users.OfType<AppUser>())
            {
                if (user.UserName != null) user.UserName = user.UserName.ToLower();
                user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
                user.LastActiveAt = DateTime.SpecifyKind(user.LastActiveAt, DateTimeKind.Utc);
                var result = await userManager.CreateAsync(user, "Password1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                    continue;
                }
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error creating user: {error.Description}");
                }
            }
        }

        var admin = new AppUser
        {
            UserName = "admin",
            DateOfBirth = new DateOnly(2000, 10, 15),
            KnownAs = "Administrator",
            Gender = "male",
            City = "Bangkok",
            Country = "Thailand"
        };

        await userManager.CreateAsync(admin, "Password1");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }
}