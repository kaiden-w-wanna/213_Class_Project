using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace spa_web_app.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = default!;
        public IdentityUser Customer { get; set; } = default!;

        // ?? CHANGED: Employee -> Therapist
        public string? TherapistId { get; set; }
        public IdentityUser? Therapist { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = default!;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndTime { get; set; }

        [Range(0, 10000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Booked";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
