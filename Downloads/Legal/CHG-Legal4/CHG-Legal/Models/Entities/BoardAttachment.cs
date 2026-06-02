using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("Board_Attachments")]
    public class BoardAttachment
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [ForeignKey("Board")]
        [Column("Board_id")]
        public int Board_id { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public int user_id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("file_name")]
        public string file_name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("file_path")]
        public string file_path { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("file_type")]
        public string? file_type { get; set; }

        [Column("file_size")]
        public long? file_size { get; set; }

        [StringLength(100)]
        [Column("mime_type")]
        public string? mime_type { get; set; }

        [Column("description", TypeName = "text")]
        public string? description { get; set; }

        [Column("uploaded_at")]
        public DateTime uploaded_at { get; set; } = DateTime.Now;

        [Column("file_data")]
        public byte[]? file_data { get; set; }

        // Navigation Properties
        public virtual Board? Board { get; set; }
        public virtual User? User { get; set; }
    }
}