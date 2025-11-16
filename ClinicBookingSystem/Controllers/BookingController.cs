using ClinicBookingSystem.Data;
using ClinicBookingSystem.Models;
using ClinicBookingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ClinicDbContext _context;

        public BookingController(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> SelectDoctor(string? specialization)
        {
            var query = _context.Doctors.AsQueryable();
            if (!string.IsNullOrEmpty(specialization))
            {
                query = query.Where(d => d.Specialization == specialization);
                ViewBag.SelectedSpecialization = specialization;
            }
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> SelectSlot(int doctorId, DateTime? date)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null) return NotFound();

            // Default to today if no date selected
            var selectedDate = date?.Date ?? DateTime.Today;
            if (selectedDate < DateTime.Today) selectedDate = DateTime.Today; // Prevent past dates

            var viewModel = new BookAppointmentViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.Name,
                Specialization = doctor.Specialization,
                SelectedDate = selectedDate,
                AvailableSlots = new List<TimeSpan>()
            };

            // --- CORE LOGIC: Generate Available Slots ---

            // 1. Check if doctor works on this day of the week
            var daySchedule = doctor.Schedules?.FirstOrDefault(s => s.DayOfWeek == selectedDate.DayOfWeek);

            if (daySchedule != null)
            {
                // 2. Generate all possible slots for this day
                var allSlots = new List<TimeSpan>();
                for (var time = daySchedule.StartTime; time <= daySchedule.EndTime; time = time.Add(TimeSpan.FromMinutes(doctor.SlotDurationMinutes)))
                {
                    allSlots.Add(time);
                }

                // 3. Get existing confirmed/pending appointments for this doctor on this date
                var existingAppointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                                a.AppointmentDateTime.Date == selectedDate.Date &&
                                a.Status != AppointmentStatus.Cancelled)
                    .Select(a => a.AppointmentDateTime.TimeOfDay)
                    .ToListAsync();

                // 4. Filter out taken slots
                viewModel.AvailableSlots = allSlots.Except(existingAppointments).ToList();

                // 5. If today, filter out past times
                if (selectedDate == DateTime.Today)
                {
                    viewModel.AvailableSlots = viewModel.AvailableSlots
                       .Where(t => t > DateTime.Now.TimeOfDay)
                       .ToList();
                }
            }

            return View(viewModel);
        }

        // Step 4: Review and Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReviewBooking(BookAppointmentViewModel model)
        {
            // Just pass the model to the review view to get patient details
            if (model.SelectedTime == null)
            {
                // If no time selected, go back to slot selection
                return RedirectToAction(nameof(SelectSlot), new { doctorId = model.DoctorId, date = model.SelectedDate });
            }
            return View(model);
        }

        // FINAL STEP: Save Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Find or Create Patient
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == model.PatientPhone);
                if (patient == null)
                {
                    patient = new Patient
                    {
                        Name = model.PatientName,
                        PhoneNumber = model.PatientPhone,
                        Email = model.PatientEmail
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }

                // 2. Create Appointment
                var appointmentDateTime = model.SelectedDate.Date + model.SelectedTime!.Value;

                // Double-check availability to prevent race conditions
                bool isTaken = await _context.Appointments.AnyAsync(a =>
                    a.DoctorId == model.DoctorId &&
                    a.AppointmentDateTime == appointmentDateTime &&
                    a.Status != AppointmentStatus.Cancelled);

                if (isTaken)
                {
                    ModelState.AddModelError("", "Sorry, this slot was just booked by someone else.");
                    return View("ReviewBooking", model);
                }

                var appointment = new Appointment
                {
                    DoctorId = model.DoctorId,
                    PatientId = patient.Id,
                    AppointmentDateTime = appointmentDateTime,
                    Status = AppointmentStatus.Pending
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(BookingConfirmation), new { appointmentId = appointment.Id });
            }

            return View("ReviewBooking", model);
        }

        public async Task<IActionResult> BookingConfirmation(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            return View(appointment);
        }
    }
}