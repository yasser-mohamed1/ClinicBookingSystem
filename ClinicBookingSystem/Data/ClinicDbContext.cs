using ClinicBookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicBookingSystem.Data
{
    public class ClinicDbContext : IdentityDbContext<ApplicationUser>
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DoctorSchedule>()
                .HasIndex(s => new { s.DoctorId, s.DayOfWeek, s.StartTime, s.EndTime })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
