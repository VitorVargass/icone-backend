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


        [Required]
        [Column("company_id")]
        public long CompanyId { get; set; }


        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        [Column("role")]
        public string Role { get; set; } = null!; // Ex: Manager or user

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
