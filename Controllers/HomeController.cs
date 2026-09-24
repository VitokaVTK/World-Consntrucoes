using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;
using World_Consntrucoes.ViewModels;

namespace World_Consntrucoes.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? category, decimal? maxPrice)
    {
        var query = db.PropertyListings
            .Where(x => x.IsPublished)
            .Include(x => x.Photos.Where(photo => photo.IsPublic))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);
        if (maxPrice.HasValue && maxPrice.Value > 0)
            query = query.Where(x => x.Price <= maxPrice.Value);

        var model = new HomeIndexViewModel
        {
            Properties = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(),
            Category = category,
            MaxPrice = maxPrice
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await db.PropertyListings
            .Include(x => x.AssignedBroker)
            .Include(x => x.Photos.Where(photo => photo.IsPublic))
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished);

        return property is null ? NotFound() : View(property);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inquiry(InquiryInput input)
    {
        var property = await db.PropertyListings
            .FirstOrDefaultAsync(x => x.Id == input.PropertyListingId && x.IsPublished);

        if (property is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Confira os dados do formulário e tente novamente.";
            return RedirectToAction(nameof(Details), new { id = input.PropertyListingId });
        }

        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;

        var lead = new ContactLead
        {
            PropertyListingId = property.Id,
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            Phone = input.Phone.Trim(),
            ClientUserId = userId,
            AssignedBrokerId = property.AssignedBrokerId,
            Status = "Novo"
        };
        lead.Messages.Add(new LeadMessage
        {
            SenderUserId = userId,
            SenderName = input.FullName.Trim(),
            Body = input.Message.Trim()
        });

        db.ContactLeads.Add(lead);
        await db.SaveChangesAsync();

        TempData["Success"] = "Mensagem enviada. A equipe da World Construções entrará em contato.";
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    public IActionResult Error() => View();
}