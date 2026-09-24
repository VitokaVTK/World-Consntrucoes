using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;
using World_Consntrucoes.ViewModels;

namespace World_Consntrucoes.Controllers;

[Authorize(Roles = RoleNames.Client)]
public class ClientController(AppDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clientId = userManager.GetUserId(User)!;
        var properties = await db.ClientPropertyAccesses
            .Where(x => x.ClientId == clientId)
            .Select(x => x.PropertyListing)
            .Include(x => x.ConstructionUpdates.OrderBy(update => update.CreatedAtUtc))
                .ThenInclude(update => update.Photo)
            .Include(x => x.AssignedBroker)
            .ToListAsync();

        var leads = await db.ContactLeads
            .Where(x => x.ClientUserId == clientId)
            .Include(x => x.PropertyListing)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return View(new ClientDashboardViewModel { Properties = properties, Leads = leads });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int id, string? body)
    {
        var clientId = userManager.GetUserId(User)!;
        var lead = await db.ContactLeads.FirstOrDefaultAsync(x => x.Id == id && x.ClientUserId == clientId);
        if (lead is null) return NotFound();
        if (string.IsNullOrWhiteSpace(body) || body.Length > 3000)
        {
            TempData["Error"] = "Escreva uma mensagem de até 3.000 caracteres.";
            return RedirectToAction(nameof(Index));
        }

        var client = await userManager.GetUserAsync(User);
        db.LeadMessages.Add(new LeadMessage
        {
            ContactLeadId = id,
            SenderUserId = clientId,
            SenderName = client?.FullName ?? "Cliente",
            Body = body.Trim()
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}