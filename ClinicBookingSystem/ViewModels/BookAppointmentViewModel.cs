using System.ComponentModel.DataAnnotations;

namespace ClinicBookingSystem.ViewModels
{
    public class BookAppointmentViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime SelectedDate { get; set; }

        public List<TimeSpan> AvailableSlots { get; set; } = new List<TimeSpan>();

        [Required(ErrorMessage = "Please select a time slot")]
        public TimeSpan? SelectedTime { get; set; }

        [Required(ErrorMessage = "Your Name is required")]
        public string PatientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        public string PatientPhone { get; set; } = string.Empty;

        [EmailAddress]
        public string? PatientEmail { get; set; }
    }
}
