using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;

namespace World_Consntrucoes.Services;

public class DatabaseInitializer(
    AppDbContext db,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
{
    public async Task InitializeAsync()
    {
        await db.Database.EnsureCreatedAsync();

        foreach (var role in new[] { RoleNames.Administrator, RoleNames.Broker, RoleNames.Client })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Não foi possível criar o perfil {role}: {string.Join(", ", result.Errors.Select(x => x.Description))}");
                }
            }
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, RoleNames.Administrator))
                throw new InvalidOperationException(
                    "O e-mail definido em BootstrapAdmin:Email já pertence a uma conta sem perfil de administrador. Use outro e-mail para o primeiro administrador.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Administrador World Construções",
            EmailConfirmed = true
        };
        var created = await userManager.CreateAsync(admin, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar o administrador inicial: {string.Join(", ", created.Errors.Select(x => x.Description))}");
        }

        var roleResult = await userManager.AddToRoleAsync(admin, RoleNames.Administrator);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Não foi possível atribuir o perfil de administrador: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
        }
    }
}