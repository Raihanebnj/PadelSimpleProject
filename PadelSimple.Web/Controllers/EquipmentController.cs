using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelSimple.Models.Data;
using PadelSimple.Web.ViewModels.Equipment;
using System.Linq;
using System.Threading.Tasks;

namespace PadelSimple.Web.Controllers;

[Authorize] // user moet ingelogd zijn
public class EquipmentController : Controller
{
    private readonly AppDbContext _db;

    public EquipmentController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.Equipment
            .OrderBy(e => e.Name)
            .Select(e => new EquipmentRowVm
            {
                Id = e.Id,
                Name = e.Name,
                TotalQuantity = e.TotalQuantity,
                AvailableQuantity = e.AvailableQuantity,
                IsActive = e.IsActive
            })
            .ToListAsync();

        var vm = new EquipmentIndexVm { Items = items };
        return View(vm);
    }
}
