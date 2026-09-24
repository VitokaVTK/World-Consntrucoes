using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using World_Consntrucoes.Data;
using World_Consntrucoes.Models;
using World_Consntrucoes.ViewModels;

namespace World_Consntrucoes.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AdminController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            PropertyCount = await db.PropertyListings.CountAsync(),
            PublishedCount = await db.PropertyListings.CountAsync(x => x.IsPublished),
            NewLeadCount = await db.ContactLeads.CountAsync(x => x.Status == "Novo"),
            Properties = await db.PropertyListings.OrderByDescending(x => x.CreatedAtUtc).Take(8).ToListAsync(),
            RecentLeads = await db.ContactLeads.Include(x => x.PropertyListing)
                .OrderByDescending(x => x.CreatedAtUtc).Take(8).ToListAsync()
        };
        return View(model);
    }

    public async Task<IActionResult> CreateProperty()
    {
        var model = new PropertyFormViewModel();
        await PopulateBrokers(model);
        return View("PropertyForm", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(PropertyFormViewModel input)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBrokers(input);
            return View("PropertyForm", input);
        }

        var listing = new PropertyListing();
        Apply(input, listing);
        db.PropertyListings.Add(listing);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Property), new { id = listing.Id });
    }

    public async Task<IActionResult> EditProperty(int id)
    {
        var property = await db.PropertyListings.FindAsync(id);
        if (property is null) return NotFound();

        var model = new PropertyFormViewModel
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Category = property.Category,
            Price = property.Price,
            City = property.City,
            Neighborhood = property.Neighborhood,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            ParkingSpaces = property.ParkingSpaces,
            AreaM2 = property.AreaM2,
            IsPublished = property.IsPublished,
            AssignedBrokerId = property.AssignedBrokerId
        };
        await PopulateBrokers(model);
        return View("PropertyForm", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProperty(int id, PropertyFormViewModel input)
    {
        if (id != input.Id) return BadRequest();
        var property = await db.PropertyListings.FindAsync(id);
        if (property is null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateBrokers(input);
            return View("PropertyForm", input);
        }

        Apply(input, property);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Property), new { id });
    }

    public async Task<IActionResult> Property(int id)
    {
        var property = await db.PropertyListings
            .Include(x => x.AssignedBroker)
            .Include(x => x.Photos.OrderByDescending(photo => photo.CreatedAtUtc))
            .Include(x => x.ConstructionUpdates.OrderByDescending(update => update.CreatedAtUtc))
                .ThenInclude(update => update.Photo)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (property is null) return NotFound();

        return View(new AdminPropertyDetailsViewModel
        {
            Property = property,
            Brokers = (await userManager.GetUsersInRoleAsync(RoleNames.Broker)).ToList(),
            Clients = (await userManager.GetUsersInRoleAsync(RoleNames.Client)).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(int propertyId, IFormFile? file, string? caption, bool isPublic)
    {
        var property = await db.PropertyListings.FindAsync(propertyId);
        if (property is null) return NotFound();
        if (file is null || file.Length == 0 || file.Length > 8 * 1024 * 1024)
        {
            TempData["Error"] = "Escolha uma imagem de até 8 MB.";
            return RedirectToAction(nameof(Property), new { id = propertyId });
        }

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        var contentType = DetectImageType(bytes);
        if (contentType is null)
        {
            TempData["Error"] = "Envie uma imagem JPG, PNG ou WebP válida.";
            return RedirectToAction(nameof(Property), new { id = propertyId });
        }

        var extension = contentType switch { "image/jpeg" => ".jpg", "image/png" => ".png", _ => ".webp" };
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(uploadDirectory);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadDirectory, fileName), bytes);

        db.PropertyPhotos.Add(new PropertyPhoto
        {
            PropertyListingId = propertyId,
            StorageFileName = fileName,
            ContentType = contentType,
            Caption = (caption ?? string.Empty).Trim(),
            IsPublic = isPublic,
            UploadedById = userManager.GetUserId(User)!
        });
        await db.SaveChangesAsync();

        TempData["Success"] = isPublic
            ? "Foto pública adicionada ao anúncio."
            : "Foto privada adicionada para a equipe e os clientes vinculados.";
        return RedirectToAction(nameof(Property), new { id = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddConstructionUpdate(int propertyId, string? title,
        string? description, int progressPercent, int? photoId)
    {
        var property = await db.PropertyListings.FindAsync(propertyId);
        if (property is null) return NotFound();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)
            || title.Length > 120 || description.Length > 2000 || progressPercent is < 0 or > 100)
        {
            TempData["Error"] = "Preencha o título, a descrição e o progresso entre 0% e 100%.";
            return RedirectToAction(nameof(Property), new { id = propertyId });
        }

        if (photoId.HasValue && !await db.PropertyPhotos.AnyAsync(x =>
                x.Id == photoId.Value && x.PropertyListingId == propertyId))
        {
            TempData["Error"] = "A foto selecionada não pertence a este imóvel.";
            return RedirectToAction(nameof(Property), new { id = propertyId });
        }

        db.ConstructionUpdates.Add(new ConstructionUpdate
        {
            PropertyListingId = propertyId,
            Title = title.Trim(),
            Description = description.Trim(),
            ProgressPercent = progressPercent,
            PhotoId = photoId
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Property), new { id = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkClient(int propertyId, string? email)
    {
        if (!await db.PropertyListings.AnyAsync(x => x.Id == propertyId)) return NotFound();
        var client = string.IsNullOrWhiteSpace(email) ? null : await userManager.FindByEmailAsync(email.Trim());
        if (client is null || !await userManager.IsInRoleAsync(client, RoleNames.Client))
        {
            TempData["Error"] = "Informe o e-mail de uma conta de cliente cadastrada.";
            return RedirectToAction(nameof(Property), new { id = propertyId });
        }

        var exists = await db.ClientPropertyAccesses.AnyAsync(x =>
            x.ClientId == client.Id && x.PropertyListingId == propertyId);
        if (!exists)
        {
            db.ClientPropertyAccesses.Add(new ClientPropertyAccess
            {
                ClientId = client.Id,
                PropertyListingId = propertyId
            });
            await db.SaveChangesAsync();
        }
        TempData["Success"] = "Cliente vinculado ao imóvel.";
        return RedirectToAction(nameof(Property), new { id = propertyId });
    }

    public async Task<IActionResult> Brokers()
    {
        ViewBag.Created = TempData["BrokerCreated"];
        return View((await userManager.GetUsersInRoleAsync(RoleNames.Broker)).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBroker(CreateBrokerViewModel input)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CreateError = "Confira os dados e use uma senha com pelo menos 8 caracteres, incluindo um número.";
            return View("Brokers", (await userManager.GetUsersInRoleAsync(RoleNames.Broker)).ToList());
        }

        var broker = new ApplicationUser
        {
            UserName = input.Email.Trim(),
            Email = input.Email.Trim(),
            FullName = input.FullName.Trim(),
            PhoneNumber = input.Phone.Trim()
        };
        var created = await userManager.CreateAsync(broker, input.TemporaryPassword);
        if (!created.Succeeded)
        {
            ViewBag.CreateError = string.Join(" ", created.Errors.Select(x => x.Description));
            return View("Brokers", (await userManager.GetUsersInRoleAsync(RoleNames.Broker)).ToList());
        }

        await userManager.AddToRoleAsync(broker, RoleNames.Broker);
        TempData["BrokerCreated"] = "Conta de corretor criada.";
        return RedirectToAction(nameof(Brokers));
    }

    private async Task PopulateBrokers(PropertyFormViewModel model) =>
        model.Brokers = (await userManager.GetUsersInRoleAsync(RoleNames.Broker)).ToList();

    private static void Apply(PropertyFormViewModel input, PropertyListing property)
    {
        property.Title = input.Title.Trim();
        property.Description = input.Description.Trim();
        property.Category = input.Category;
        property.Price = input.Price;
        property.City = input.City.Trim();
        property.Neighborhood = input.Neighborhood.Trim();
        property.Bedrooms = input.Bedrooms;
        property.Bathrooms = input.Bathrooms;
        property.ParkingSpaces = input.ParkingSpaces;
        property.AreaM2 = input.AreaM2;
        property.IsPublished = input.IsPublished;
        property.AssignedBrokerId = string.IsNullOrWhiteSpace(input.AssignedBrokerId) ? null : input.AssignedBrokerId;
    }

    private static string? DetectImageType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return "image/jpeg";
        if (bytes.Length >= 8 && bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes.Length >= 12 && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
            && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP") return "image/webp";
        return null;
    }
}