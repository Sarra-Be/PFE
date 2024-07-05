namespace stage_api.DTO
{
    public class CreateValidationRequestDto
    {
        public string FileName { get; set; }
        public string TargetTableName { get; set; }
        public string AttributeMappingStr { get; set; }
        public string FileJsonStrContent { get; set; }
        public string RequestById { get; set; }
        public bool IsAdmin { get; set; }
    }
}
