using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BranchAttachments", Schema = "Company")]
    public class BranchAttachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int BranchId { get; set; }

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

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}