using ClinicBookingSystem.Data;
using ClinicBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ClinicDbContext _context;

        public ScheduleController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: Schedule/Index/5 (View schedule for a specific doctor)
        public async Task<IActionResult> Index(int? doctorId)
        {
            if (doctorId == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(m => m.Id == doctorId);

            if (doctor == null) return NotFound();

            ViewBag.DoctorId = doctor.Id;
            ViewBag.DoctorName = doctor.Name;

            return View(doctor.Schedules?.OrderBy(s => s.DayOfWeek).ToList());
        }

        // GET: Schedule/Create?doctorId=5
        public IActionResult Create(int doctorId)
        {
            ViewBag.DoctorId = doctorId;
            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId,DayOfWeek,StartTime,EndTime")] DoctorSchedule schedule)
        {
            // Basic validation: EndTime must be after StartTime
            if (schedule.EndTime <= schedule.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(schedule);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { doctorId = schedule.DoctorId });
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "A schedule for this day already exists for this doctor.");
                }
            }
            ViewBag.DoctorId = schedule.DoctorId;
            return View(schedule);
        }

        // POST: Schedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.DoctorSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { doctorId = schedule.DoctorId });
            }
            return NotFound();
        }
    }
}