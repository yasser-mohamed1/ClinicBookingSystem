using System.ComponentModel.DataAnnotations;

namespace ClinicBookingSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
