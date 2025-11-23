using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace icone_backend.Models
{
    [Table("companies")]
    public class CompaniesModel
    {

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("document")]
        public string Document { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        [Column("phone")]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("plan")]
        public string Plan { get; set; } = null!;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
