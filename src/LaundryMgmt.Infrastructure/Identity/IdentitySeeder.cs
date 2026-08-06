using LaundryMgmt.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LaundryMgmt.Infrastructure.Identity;

/// <summary>
/// Dev-only bootstrap: ensures every <see cref="UserRole"/> exists as an Identity role,
/// then creates a default Admin login and a default Customer login if neither exists yet,
/// so there's an actual account to sign in with (README calls this out as a gap — no
/// RegisterCommand/seeder existed before this). Credentials come from configuration
/// ("SeedUsers" section) with fallback defaults; override them via user-secrets/appsettings
/// for anything beyond local development.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        await EnsureUserAsync(
            userManager, logger,
            userName: configuration["SeedUsers:Admin:UserName"] ?? "admin",
            email: configuration["SeedUsers:Admin:Email"] ?? "admin@laundrymgmt.local",
            password: configuration["SeedUsers:Admin:Password"] ?? "Admin#12345",
            fullName: configuration["SeedUsers:Admin:FullName"] ?? "System Administrator",
            role: UserRole.Admin);

        await EnsureUserAsync(
            userManager, logger,
            userName: configuration["SeedUsers:Customer:UserName"] ?? "customer",
            email: configuration["SeedUsers:Customer:Email"] ?? "customer@laundrymgmt.local",
            password: configuration["SeedUsers:Customer:Password"] ?? "Customer#12345",
            fullName: configuration["SeedUsers:Customer:FullName"] ?? "Demo Customer",
            role: UserRole.Customer);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager, ILogger logger,
        string userName, string email, string password, string fullName, UserRole role)
    {
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Role = role,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning(
                "Seed user {UserName} was not created: {Errors}",
                userName, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role.ToString());
        logger.LogInformation("Seeded {Role} login {UserName} ({Email})", role, userName, email);
    }
}
