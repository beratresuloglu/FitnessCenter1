using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [NotMapped]
        public DateTime AppointmentStartDateTime => AppointmentDate.Date.Add(StartTime);

        [NotMapped]
        public DateTime AppointmentEndDateTime => AppointmentDate.Date.Add(EndTime);

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public bool IsApproved { get; set; } = false;

        public DateTime? ApprovedDate { get; set; }

        public string? ApprovedBy { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Foreign Keys
        [Required]
        public int MemberId { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(MemberId))]
        public Member Member { get; set; } = null!;

        [ForeignKey(nameof(TrainerId))]
        public Trainer Trainer { get; set; } = null!;

        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; } = null!;
    }
}