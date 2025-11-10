using System.ComponentModel.DataAnnotations;

namespace ClinicBookingSystem.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDateTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
