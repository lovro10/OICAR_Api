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

        // GET: api/Image
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetUserImages()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            var images = await _context.Images
                .Where(i => i.KorisnikImagelices.Any(u => u.Idkorisnik == userId))
                .ToListAsync();
            return Ok(images);
        }

        // GET: api/Image/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            var image = await _context.Images
                .Include(i => i.KorisnikImagelices)
                .FirstOrDefaultAsync(i => i.Idimage == id);
            if (image == null)
                return NotFound();

            if (!image.KorisnikImagelices.Any(u => u.Idkorisnik == userId))
                return Forbid();

            return Ok(image);
        }

        // POST: api/Image
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            byte[] content;
            try
            {
                content = Convert.FromBase64String(dto.Base64Content);
            }
            catch (Exception)
            {
                return BadRequest("Invalid Base64 content.");
            }

            var image = new Image
            {
                Name = dto.Name,
                Content = content
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            // Retrieve the current user
            var user = await _context.Korisniks.FirstOrDefaultAsync(u => u.Idkorisnik == userId);
            if (user == null)
                return Unauthorized();

            user.Imagelice = image;  //  user.Imagevozacka = image;  user.Imageosobna = image;
            _context.Korisniks.Update(user);
            await _context.SaveChangesAsync();


            return Ok(new { message = "Image uploaded successfully", imageId = image.Idimage });
        }

        // PUT: api/Image/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] ImageUploadDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            var image = await _context.Images
                .Include(i => i.KorisnikImagelices)
                .FirstOrDefaultAsync(i => i.Idimage == id);
            if (image == null)
                return NotFound();

            if (!image.KorisnikImagelices.Any(u => u.Idkorisnik == userId))
                return Forbid();

            image.Name = dto.Name;
            try
            {
                image.Content = Convert.FromBase64String(dto.Base64Content);
            }
            catch (Exception)
            {
                return BadRequest("Invalid Base64 content.");
            }

            _context.Entry(image).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image updated successfully" });
        }

        // DELETE: api/Image/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();
            int userId = int.Parse(userIdString);

            var image = await _context.Images
                .Include(i => i.KorisnikImagelices)
                .FirstOrDefaultAsync(i => i.Idimage == id);
            if (image == null)
                return NotFound();

            if (!image.KorisnikImagelices.Any(u => u.Idkorisnik == userId))
                return Forbid();

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image deleted successfully" });
        }
    }
}
