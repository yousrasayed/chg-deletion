using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CHG_Legal.Models.Entities
{
    [Table("BoardType")]
    public class BoardType
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("BoardType")]
        public string BoardTypeName { get; set; } = string.Empty;

        // Navigation Properties
        [JsonIgnore]
        public virtual ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}