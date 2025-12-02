namespace PIT.Common.Models
{
    public class BasicResponseDTO
    {
        public BasicResponseDTO() { }
        public BasicResponseDTO(BasicEntity entity) 
        {
            Id = entity.Id;
            DisplayName = entity.DisplayName;
        }
        public Guid? Id { get; set; } = default!;
        public string? DisplayName { get; set; } = default!;
    }
}
