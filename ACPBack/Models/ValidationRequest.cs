using System.ComponentModel.DataAnnotations;

namespace stage_api.Models
{
    public class ValidationRequest
    {
        [Key]
        public int? Id { get; set; }
        public string? FileName { get; set; }
        public string? TargetTableName { get; set; }
        public bool? IsValidated { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ValidationDate { get; set; }
        public string AttributeMappingStr { get; set; }
        public string FileJsonStrContent { get; set; }
        public ApplicationUser RequestedBy { get; set; }
    }
}
