using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using System.Security.Claims;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ImageController : ControllerBase
    {
        private readonly CarshareContext _context;

        public ImageController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet]
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

        // GET: api/Image/5
        [HttpGet("{id}")]
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

        // POST: api/Image
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Base64Content))
                return BadRequest("Base64 content is required.");

            try
            {
                var image = new Image
                {
                    Name = dto.Name,
                    Content = Convert.FromBase64String(dto.Base64Content)
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetImageById), new { id = image.Idimage }, dto);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string.");
            }
        }

        // PUT: api/Image/5
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
                return NoContent();
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string.");
            }
        }

        // DELETE: api/Image/5
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
