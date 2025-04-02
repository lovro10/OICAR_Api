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
    [Authorize] // Requires a valid JWT token
    public class VoziloController : ControllerBase
    {
        private readonly CarshareContext _context;

        public VoziloController(CarshareContext context)
        {
            _context = context;
        }

        // GET: api/Vozilo
        // Returns all vehicles registered by the current driver.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vozilo>>> GetVozila()
        {
            var driverIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverIdString))
                return Unauthorized("Niste autorizirani.");
            int driverId = int.Parse(driverIdString);

            // Query Oglasvozilo to find vehicles associated with this driver.
            var vehicles = await _context.Oglasvozilos
                .Where(ov => ov.Korisnikid == driverId && ov.Voziloid != null)
                .Include(ov => ov.Vozilo)
                .Select(ov => ov.Vozilo)
                .ToListAsync();

            return Ok(vehicles);
        }

        // GET: api/Vozilo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Vozilo>> GetVozilo(int id)
        {
            var vozilo = await _context.Vozilos.FindAsync(id);
            if (vozilo == null)
                return NotFound();
            return Ok(vozilo);
        }

        // POST: api/Vozilo
        // Creates a new vehicle and associates it with the current driver.
        [HttpPost]
        public async Task<IActionResult> CreateVozilo([FromBody] VoziloDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get current driver's id.
            var driverIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverIdString))
                return Unauthorized("Niste autorizirani.");
            int driverId = int.Parse(driverIdString);

            // Check if a vehicle with the same registration already exists.
            if (await _context.Vozilos.AnyAsync(v => v.Registracija == dto.Registracija))
                return BadRequest("Vozilo sa ovom registracijom već postoji.");

            // Create a new Vozilo record.
            var vehicle = new Vozilo
            {
                Marka = dto.Marka,
                Model = dto.Model,
                Registracija = dto.Registracija,
                Imageprometnaid = dto.Imageprometnaid
            };

            _context.Vozilos.Add(vehicle);
            await _context.SaveChangesAsync();

            // Create an ownership record in Oglasvozilo.
            var oglasVozilo = new Oglasvozilo
            {
                Korisnikid = driverId,
                Voziloid = vehicle.Idvozilo
            };

            _context.Oglasvozilos.Add(oglasVozilo);
            await _context.SaveChangesAsync();

            return Ok(new { poruka = "Vozilo uspješno kreirano.", id = vehicle.Idvozilo });
        }

        // PUT: api/Vozilo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVozilo(int id, [FromBody] VoziloUpdateDTO dto)
        {
            if (id != dto.Idvozilo)
                return BadRequest("ID se ne podudaraju.");

            var vehicle = await _context.Vozilos.FindAsync(id);
            if (vehicle == null)
                return NotFound();

            // Update the vehicle properties.
            vehicle.Marka = dto.Marka;
            vehicle.Model = dto.Model;
            vehicle.Registracija = dto.Registracija;
            vehicle.Imageprometnaid = dto.Imageprometnaid;

            _context.Entry(vehicle).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Vozilos.AnyAsync(v => v.Idvozilo == id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(new { poruka = "Vozilo uspješno ažurirano." });
        }

        // DELETE: api/Vozilo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVozilo(int id)
        {
            var vehicle = await _context.Vozilos.FindAsync(id);
            if (vehicle == null)
                return NotFound();

            // Delete associated Oglasvozilo records.
            var oglasRecords = _context.Oglasvozilos.Where(ov => ov.Voziloid == id);
            _context.Oglasvozilos.RemoveRange(oglasRecords);

            _context.Vozilos.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { poruka = "Vozilo uspješno obrisano." });
        }
    }
}
