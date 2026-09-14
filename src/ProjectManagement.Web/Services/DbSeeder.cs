using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;

namespace ProjectManagement.Web.Services;

/// <summary>
/// 啟動時的初始化資料：角色、預設管理員帳號、示範用單位與系統類別。
/// 注意：這裡只負責「灌資料」，資料庫結構本身仍須先用 `dotnet ef database update` 建立。
/// 預設管理員密碼僅供第一次登入使用，請務必在正式環境改掉，或改用環境變數/User Secrets 覆寫。
/// </summary>
public static class DbSeeder
{
    private const string DefaultAdminEmail = "admin@projectmanagement.local";
    private const string DefaultAdminPassword = "Admin#12345";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
                new Department { Name = "資訊部" },
                new Department { Name = "生產部" },
                new Department { Name = "品保部" });
        }

        if (!db.SystemCategories.Any())
        {
            db.SystemCategories.AddRange(
                new SystemCategory { Name = "SFCS" },
                new SystemCategory { Name = "機況" },
                new SystemCategory { Name = "標籤" },
                new SystemCategory { Name = "品質" });
        }

        await db.SaveChangesAsync();

        var adminUser = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                EmailConfirmed = true,
                DisplayName = "系統管理員",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
        }
    }
}
