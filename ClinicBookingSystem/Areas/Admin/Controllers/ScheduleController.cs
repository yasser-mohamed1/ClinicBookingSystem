using ClinicBookingSystem.Data;
using ClinicBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ScheduleController : Controller
    {
        private readonly ClinicDbContext _context;

        public ScheduleController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: Schedule/Index/5 (View schedule for a specific doctor)
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null) return NotFound();

            ViewBag.DoctorId = doctor.Id;
            ViewBag.DoctorName = doctor.Name;

            return View(doctor.Schedules?.OrderBy(s => s.DayOfWeek).ToList());
        }

        // GET: Schedule/Create/5
        public IActionResult Create(int id)
        {
            ViewBag.DoctorId = id;
            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId,DayOfWeek,StartTime,EndTime")] DoctorSchedule schedule)
        {
            if (schedule.EndTime <= schedule.StartTime)
                ModelState.AddModelError("EndTime", "End time must be after start time.");

            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = schedule.DoctorId });
            }

            ViewBag.DoctorId = schedule.DoctorId;
            return View(schedule);
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);

            if (schedule != null)
            {
                int doctorId = schedule.DoctorId;

                _context.DoctorSchedules.Remove(schedule);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id = doctorId });
            }

            return NotFound();
        }
    }
}