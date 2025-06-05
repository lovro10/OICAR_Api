namespace REST_API___oicar.DTOs
{
    public class ImageDTO
    {
        public int Idimage { get; set; }
        public string Name { get; set; }
        public string ContentBase64 { get; set; }
        public int? ImageTypeId { get; set; }
        public string? ImageTypeName { get; set; }
    }
}
