using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;

namespace World_Consntrucoes.Controllers;

public class MediaController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Photo(int id)
    {
        var photo = await db.PropertyPhotos
            .Include(x => x.PropertyListing)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (photo is null) return NotFound();

        if (!photo.IsPublic)
        {
            if (User.Identity?.IsAuthenticated != true) return Challenge();

            var allowed = User.IsInRole(RoleNames.Administrator);
            var userId = userManager.GetUserId(User);
            if (!allowed && User.IsInRole(RoleNames.Broker))
                allowed = photo.PropertyListing.AssignedBrokerId == userId;
            if (!allowed && User.IsInRole(RoleNames.Client))
                allowed = await db.ClientPropertyAccesses.AnyAsync(x =>
                    x.ClientId == userId && x.PropertyListingId == photo.PropertyListingId);
            if (!allowed) return Forbid();
        }

        var path = Path.Combine(environment.ContentRootPath, "App_Data", "uploads", photo.StorageFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, photo.ContentType);
    }
}