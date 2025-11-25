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
        public string FantasyName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("corporate_name")]
        public string CorporateName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("document")]
        public string Document { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        [Column("phone")]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("website")]
        public string Website { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("plan")]
        public string Plan { get; set; } = null!;

        [Required]
        [MaxLength(4)]
        [Column("country_code")]
        public string CountryCode { get; set; } = null!;

        
        [Required]
        [MaxLength(20)]
        [Column("postal_code")]
        public string PostalCode { get; set; } = null!;

        
        [Required]
        [MaxLength(100)]
        [Column("state_region")]
        public string StateRegion { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("city")]
        public string City { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("line1")]
        public string Line1 { get; set; } = null!;
       
        [MaxLength(150)]
        [Column("line2")]
        public string? Line2 { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
