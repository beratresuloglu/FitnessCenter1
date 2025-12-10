using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterWebApplication.Models.Entities
{
    public class WorkoutPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string AIRecommendation { get; set; } = string.Empty;

        public string? UserInputData { get; set; }

        public string? UploadedImageUrl { get; set; }

        public string? GeneratedImageUrl { get; set; }

        [StringLength(100)]
        public string? PlanType { get; set; }

        public int? DurationWeeks { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModifiedDate { get; set; }

        // Foreign Keys
        [Required]
        public int MemberId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(MemberId))]
        public Member Member { get; set; } = null!;
    }
}