using System.ComponentModel.DataAnnotations;

namespace stage_api.Models
{
    public class ChatMessage
    {
        [Key]
        public int? Id { get; set; }
        public string? Message { get; set; }
        public DateTime? CreationDate { get; set; }
        public virtual ApplicationUser Owner { get; set; }
    }
}
