using Microsoft.AspNetCore.Mvc;
using ClinicBookingSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ClinicDbContext _context;

    public HomeController(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var specializations = await _context.Doctors
            .Select(d => d.Specialization)
            .Distinct()
            .ToListAsync();

        return View(specializations);
    }
}
