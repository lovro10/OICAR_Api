using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using System.Collections;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoziloController : ControllerBase
    {
        private readonly CarshareContext _context;

        public VoziloController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Vozilo>>> GetAll()
        {
            var vozila = await _context.Vozilos
                .Include(v => v.Imageprometna)
                .Select(v => new
                {
                    v.Idvozilo,
                    v.Marka,
                    v.Model,
                    v.Registracija,
                    Imageprometna = new
                    {
                        v.Imageprometna.Idimage,
                        v.Imageprometna.Name,
                        v.Imageprometna.Content
                    }
                })
                .ToListAsync();

            return Ok(vozila);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Vozilo>> GetById(int id)
        {
            var vozilo = await _context.Vozilos
                .Where(v => v.Idvozilo == id)
                .Include(v => v.Imageprometna)
                .Select(v => new
                {
                    v.Idvozilo,
                    v.Marka,
                    v.Model,
                    v.Registracija,
                    Imageprometna = new
                    {
                        v.Imageprometna.Name,
                        v.Imageprometna.Content
                    }
                })
                .FirstOrDefaultAsync();

            if (vozilo == null)
                return NotFound();

            return Ok(vozilo);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<VoziloDTO>> KrerirajVozilo([FromBody] VoziloDTO voziloDTO)
        {
            try
            {
                var vozilo = new Vozilo
                {
                    Marka = voziloDTO.Marka,
                    Model = voziloDTO.Model,
                    Registracija = voziloDTO.Registracija
                };

                vozilo.Imageprometna = await SaveImageFromBase64Async(voziloDTO.Prometna, $"Prometna {voziloDTO.Registracija}");

                _context.Vozilos.Add(vozilo);
                await _context.SaveChangesAsync();

                voziloDTO.Id = vozilo.Idvozilo;

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<Image> SaveImageFromBase64Async(string base64Image, string name)
        {
            if (string.IsNullOrEmpty(base64Image))
                return null;

            try
            {
                byte[] imageData = Convert.FromBase64String(base64Image);

                var image = new Image
                {
                    Name = name,
                    Content = imageData
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return image;
            }
            catch (FormatException)
            {
                throw new Exception($"Neispravan base64 format za sliku '{name}'.");
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<VoziloDTO>> Details(int id)
        {
            try
            {
                var vozilo = await _context.Vozilos
                    .Include(v => v.Imageprometna)
                    .FirstOrDefaultAsync(v => v.Idvozilo == id);

                if (vozilo == null)
                {
                    return NotFound($"Vozilo sa ID-jem {id} nije pronađeno.");
                }

                var voziloDTO = new VoziloDTO
                {
                    Id = vozilo.Idvozilo,
                    Marka = vozilo.Marka,
                    Model = vozilo.Model,
                    Registracija = vozilo.Registracija,
                    Prometna = vozilo.Imageprometna != null ? Convert.ToBase64String(vozilo.Imageprometna.Content) : null
                };

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult<VoziloDTO>> UpdateVozilo(int id, [FromBody] VoziloDTO voziloDTO)
        {
            try
            {
                var vozilo = await _context.Vozilos.FindAsync(id);

                if (vozilo == null)
                {
                    return NotFound($"Vozilo sa ID-jem {id} nije pronađeno.");
                }

                vozilo.Marka = voziloDTO.Marka;
                vozilo.Model = voziloDTO.Model;
                vozilo.Registracija = voziloDTO.Registracija;

                if (!string.IsNullOrEmpty(voziloDTO.Prometna))
                {
                    vozilo.Imageprometna = await SaveImageFromBase64Async(voziloDTO.Prometna, $"Prometna {voziloDTO.Registracija}");
                }

                _context.Vozilos.Update(vozilo);
                await _context.SaveChangesAsync();

                voziloDTO.Id = vozilo.Idvozilo;

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vozilo = await _context.Vozilos.FindAsync(id);
            if (vozilo == null)
                return NotFound();

            _context.Vozilos.Remove(vozilo);
            await _context.SaveChangesAsync();

            return Ok(vozilo);
        }
    }
}

