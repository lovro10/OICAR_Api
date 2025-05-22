using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly CarshareContext _context;

        public ImageController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")] 
        public async Task<ActionResult<ImageDisplayDTO>> DisplayImage(int id)
        { 
            var image = await _context.Images
                .Include(x => x.Imagetype)
                .FirstOrDefaultAsync(x => x.Idimage == id); 

            if (image == null)
                return NotFound();

            var base64 = Convert.ToBase64String(image.Content);

            var imageDisplayDTO = new ImageDisplayDTO 
            { 
                Name = image.Name,
                Content = base64, 
                Type = image.Imagetype?.Name
            }; 

            return Ok(imageDisplayDTO);
        }

        [HttpGet("[action]")] 
        public async Task<ActionResult<IEnumerable<ImageDisplayDTO>>> GetImagesForUser(int userId)
        { 
            var images = await _context.Korisnikimages
                .Where(x => x.Korisnikid == userId)
                .Include(x => x.Image)
                .ThenInclude(x => x.Imagetype)
                .Select(x => new ImageDisplayDTO
                { 
                    Name = x.Image.Name,
                    Content = Convert.ToBase64String(x.Image.Content),
                    Type = x.Image.Imagetype.Name 
                })
                .ToListAsync();

            return Ok(images);
        }

        [HttpGet("[action]")] 
        public async Task<ActionResult<IEnumerable<ImageDisplayDTO>>> GetImagesForVehicle(int vehicleId)
        {
            var vehicle = await _context.Vozilos
                .FirstOrDefaultAsync(v => v.Idvozilo == vehicleId);

            if (vehicle == null)
                return NotFound();

            var registration = vehicle.Registracija.ToLower(); 

            var images = await _context.Images 
                .Where(x => 
                    x.Name.ToLower().StartsWith("Prednja") &&
                    x.Name.ToLower().EndsWith(registration))
                .Include(x => x.Imagetype)
                .Select(x => new ImageDisplayDTO
                { 
                    Name = x.Name,
                    Content = Convert.ToBase64String(x.Content),
                    Type = x.Imagetype.Name  
                })
                .ToListAsync();

            return Ok(images);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ImageUploadDTO>>> GetImages()
        {
            var images = await _context.Images.ToListAsync();

            var imageDtos = images.Select(img => new ImageUploadDTO
            {
                Name = img.Name,
                Base64Content = Convert.ToBase64String(img.Content)
            }).ToList();

            return Ok(imageDtos);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<ImageUploadDTO>> GetImageById(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            var dto = new ImageUploadDTO
            {
                Name = image.Name,
                Base64Content = Convert.ToBase64String(image.Content)
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ImageUploadDTO>> UploadImage([FromBody] ImageUploadDTO imageDto)
        {

            if (string.IsNullOrWhiteSpace(imageDto.Base64Content))
                return BadRequest("Base64 content is required.");

            try
            {
                var image = new Image
                {
                    Name = imageDto.Name,
                    Content = Convert.FromBase64String(imageDto.Base64Content),
                    Imagetypeid = imageDto.ImageTypeId
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return Ok(image.Idimage);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string.");
            }
        } 

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] ImageUploadDTO dto)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            try
            {
                image.Name = dto.Name;
                image.Content = Convert.FromBase64String(dto.Base64Content);

                await _context.SaveChangesAsync();
                return Ok(dto);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
