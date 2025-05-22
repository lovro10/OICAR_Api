namespace REST_API___oicar.DTOs
{
    public class ImageUploadDTO
    {
        public string Name { get; set; } = string.Empty;

        public string Base64Content { get; set; } = string.Empty;

        public int ImageTypeId { get; set; }
    }
}