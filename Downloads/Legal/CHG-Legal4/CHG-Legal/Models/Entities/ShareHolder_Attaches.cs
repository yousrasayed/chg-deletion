using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("ShareHolder_Attaches", Schema = "dbo")]
    public class ShareHolder_Attaches
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ShareholderId { get; set; }

        public int? CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string file_name { get; set; }

        [MaxLength(500)]
        public string? file_path { get; set; }

        [MaxLength(100)]
        public string? file_type { get; set; }

        public long? file_size { get; set; }

        [MaxLength(100)]
        public string? mime_type { get; set; }

        public string? description { get; set; }

        public DateTime? uploaded_at { get; set; }

        public byte[]? file_data { get; set; }

        public int? UserID { get; set; }

        [ForeignKey("ShareholderId")]
        public virtual Shareholder? Shareholder { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}