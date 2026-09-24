using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using World_Consntrucoes.Models;
using World_Consntrucoes.ViewModels;

namespace World_Consntrucoes.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginInput { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await userManager.FindByEmailAsync(input.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(input);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!, input.Password, input.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                result.IsLockedOut ? "A conta está temporariamente bloqueada." : "E-mail ou senha inválidos.");
            return View(input);
        }

        if (!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl))
            return Redirect(input.ReturnUrl);
        if (await userManager.IsInRoleAsync(user, RoleNames.Administrator))
            return RedirectToAction("Index", "Admin");
        if (await userManager.IsInRoleAsync(user, RoleNames.Broker))
            return RedirectToAction("Inbox", "Broker");
        return RedirectToAction("Index", "Client");
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterInput());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if (!ModelState.IsValid) return View(input);

        var user = new ApplicationUser
        {
            UserName = input.Email.Trim(),
            Email = input.Email.Trim(),
            FullName = input.FullName.Trim(),
            PhoneNumber = input.Phone.Trim()
        };
        var result = await userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(input);
        }

        await userManager.AddToRoleAsync(user, RoleNames.Client);
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Client");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}