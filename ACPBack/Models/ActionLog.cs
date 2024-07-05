using System.ComponentModel.DataAnnotations;

namespace stage_api.Models
{
    public class ActionLog
    {
        [Key]
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public virtual ApplicationUser PerformedBy { get; set; }
    }
}
