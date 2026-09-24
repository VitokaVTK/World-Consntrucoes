using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;
using World_Consntrucoes.ViewModels;

namespace World_Consntrucoes.Controllers;

[Authorize(Roles = RoleNames.Broker + "," + RoleNames.Administrator)]
public class BrokerController(AppDbContext db, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Inbox()
    {
        var query = LeadsVisibleToCurrentUser();
        var leads = await query.Include(x => x.PropertyListing)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return View(leads);
    }

    public async Task<IActionResult> Lead(int id)
    {
        var lead = await LeadsVisibleToCurrentUser()
            .Include(x => x.PropertyListing)
            .Include(x => x.Messages.OrderBy(message => message.CreatedAtUtc))
                .ThenInclude(message => message.SenderUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        return lead is null ? NotFound() : View(new LeadConversationViewModel { Lead = lead });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string? body)
    {
        var lead = await LeadsVisibleToCurrentUser().FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null) return NotFound();
        if (string.IsNullOrWhiteSpace(body) || body.Length > 3000)
        {
            TempData["Error"] = "Escreva uma mensagem de até 3.000 caracteres.";
            return RedirectToAction(nameof(Lead), new { id });
        }

        var sender = await userManager.GetUserAsync(User);
        db.LeadMessages.Add(new LeadMessage
        {
            ContactLeadId = id,
            SenderUserId = sender?.Id,
            SenderName = sender?.FullName ?? "Equipe World Construções",
            Body = body.Trim()
        });
        lead.Status = "Em atendimento";
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Lead), new { id });
    }

    private IQueryable<ContactLead> LeadsVisibleToCurrentUser()
    {
        var query = db.ContactLeads.AsQueryable();
        if (!User.IsInRole(RoleNames.Administrator))
        {
            var brokerId = userManager.GetUserId(User);
            query = query.Where(x => x.AssignedBrokerId == brokerId);
        }
        return query;
    }
}