using System.ComponentModel.DataAnnotations;

namespace ClinicBookingSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Doctor name is required")]
        [Display(Name = "Doctor Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        public string Specialization { get; set; } = string.Empty;

        [Range(5, 120, ErrorMessage = "Slot duration must be between 5 and 120 minutes")]
        [Display(Name = "Slot Duration (Minutes)")]
        public int SlotDurationMinutes { get; set; } = 30;

        public ICollection<DoctorSchedule>? Schedules { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
