using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace icone_backend.Models
{
    [Table("users")]
    public class UserModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        
        [Column("company_id")]
        public long? CompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("document")]
        public string Document { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        [Column("role")]
        public string Role { get; set; } = null!; // Ex: Manager or user

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // 🔹 Relação com CompaniesModel
        [ForeignKey("CompanyId")]
        public CompaniesModel? Company { get; set; }

    }
}
