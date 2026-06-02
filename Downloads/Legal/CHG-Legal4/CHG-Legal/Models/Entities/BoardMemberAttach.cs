using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardMemberAttaches", Schema = "Company")]
    public class BoardMemberAttach
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int? MemberId { get; set; }

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

        public int? BoardSettingBoardId { get; set; }

        [ForeignKey("MemberId")]
        public virtual BoardMember? BoardMember { get; set; }
    }
}