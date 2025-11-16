using ClinicBookingSystem.Data;
using ClinicBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Controllers
{
    // [Authorize] // Important for real apps
    public class AppointmentsController : Controller
    {
        private readonly ClinicDbContext _context;

        public AppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: Appointments (With optional filters)
        public async Task<IActionResult> Index(string? status, int? doctorId)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            // Simple filters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
                ViewBag.CurrentStatus = status;
            }

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }

            // Default sorting: Newest pending first, then by date
            var appointments = await query
                .OrderByDescending(a => a.Status == AppointmentStatus.Pending)
                .ThenByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            // Populate doctor list for filter dropdown
            ViewBag.Doctors = await _context.Doctors.ToListAsync();

            return View(appointments);
        }

        // POST: Appointments/UpdateStatus/5?status=Confirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}