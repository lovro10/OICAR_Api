using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using System.Security.Claims;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OglasVoziloController : ControllerBase
    { 
        private readonly CarshareContext _context;

        public OglasVoziloController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoznjaDTO>>> GetAll()
        {
            var oglasiVozila = await _context.Oglasvozilos 
                .Include(o => o.Korisnik) 
                .Include(o => o.Vozilo) 
                .Select(o => new OglasVoziloDTO 
                { 
                    IdOglasVozilo = o.Idoglasvozilo, 
                    VoziloId = o.Voziloid, 
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija, 
                    DatumPocetkaRezervacije = o.DatumPocetkaRezervacije, 
                    DatumZavrsetkaRezervacije = o.DatumPocetkaRezervacije, 
                    KorisnikId = o.Korisnikid, 
                    Username = o.Korisnik.Username,
                    Ime = o.Korisnik.Ime,
                    Prezime = o.Korisnik.Prezime,
                    Email = o.Korisnik.Email  
                }) 
                .ToListAsync();

            return Ok(oglasiVozila);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoznjaDTO>> GetById(int id)
        {
            var oglasVozilo = await _context.Oglasvozilos
            .Include(o => o.Korisnik)
            .Include(o => o.Vozilo)
            .Where(o => o.Idoglasvozilo == id)
            .Select(o => new OglasVoziloDTO
            {
                IdOglasVozilo = o.Idoglasvozilo,
                VoziloId = o.Voziloid,
                Marka = o.Vozilo.Marka,
                Model = o.Vozilo.Model,
                Registracija = o.Vozilo.Registracija,
                DatumPocetkaRezervacije = o.DatumPocetkaRezervacije,
                DatumZavrsetkaRezervacije = o.DatumPocetkaRezervacije,
                KorisnikId = o.Korisnikid,
                Username = o.Korisnik.Username,
                Ime = o.Korisnik.Ime,
                Prezime = o.Korisnik.Prezime,
                Email = o.Korisnik.Email
            })
            .FirstOrDefaultAsync();

            if (oglasVozilo == null)
                return NotFound();

            return Ok(oglasVozilo);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoznjaDTO>> DetaljiOglasaVoznje(int id)
        {
            var oglasVozilo = await _context.Oglasvozilos
            .Include(o => o.Korisnik)
            .Include(o => o.Vozilo)
            .Where(o => o.Idoglasvozilo == id)
            .Select(o => new OglasVoziloDTO
            {
                IdOglasVozilo = o.Idoglasvozilo,
                VoziloId = o.Voziloid,
                Marka = o.Vozilo.Marka,
                Model = o.Vozilo.Model,
                Registracija = o.Vozilo.Registracija,
                DatumPocetkaRezervacije = o.DatumPocetkaRezervacije,
                DatumZavrsetkaRezervacije = o.DatumPocetkaRezervacije,
                KorisnikId = o.Korisnikid,
                Username = o.Korisnik.Username,
                Ime = o.Korisnik.Ime,
                Prezime = o.Korisnik.Prezime,
                Email = o.Korisnik.Email
            })
            .FirstOrDefaultAsync();

            if (oglasVozilo == null)
                return NotFound();

            return Ok(oglasVozilo);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> KreirajOglasVozilo([FromBody] OglasVoziloDTO oglasVoziloDTO) 
        { 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var noviOglasVozilo = new Oglasvozilo
            {
                Voziloid = oglasVoziloDTO.VoziloId,
                DatumPocetkaRezervacije = oglasVoziloDTO.DatumPocetkaRezervacije, 
                DatumZavrsetkaRezervacije = oglasVoziloDTO.DatumZavrsetkaRezervacije, 
                Korisnikid = oglasVoziloDTO.KorisnikId
            };

            _context.Oglasvozilos.Add(noviOglasVozilo);
            await _context.SaveChangesAsync();

            oglasVoziloDTO.IdOglasVozilo = noviOglasVozilo.Idoglasvozilo;
            
            return Ok(oglasVoziloDTO);
        }

        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> AzurirajOglasVozilo(int id, [FromBody] OglasVoziloDTO oglasVoziloDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oglasVozilo = await _context.Oglasvozilos 
                .FirstOrDefaultAsync(o => o.Idoglasvozilo == id);

            if (oglasVozilo == null)
                return NotFound();

            oglasVozilo.Voziloid = oglasVoziloDTO.VoziloId;
            oglasVozilo.DatumPocetkaRezervacije = oglasVoziloDTO.DatumPocetkaRezervacije;
            oglasVozilo.DatumZavrsetkaRezervacije = oglasVoziloDTO.DatumZavrsetkaRezervacije;
            oglasVozilo.Korisnikid = oglasVoziloDTO.KorisnikId; 

            _context.Oglasvozilos.Update(oglasVozilo); 

            await _context.SaveChangesAsync();

            oglasVoziloDTO.IdOglasVozilo = oglasVozilo.Idoglasvozilo;
            
            return Ok(oglasVoziloDTO);
        } 

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        { 
            var oglasVozilo = await _context.Oglasvozilos 
                .Include(o => o.Vozilo) 
                .Include(o => o.Korisnik)
                .FirstOrDefaultAsync(o => o.Idoglasvozilo == id);

            if (oglasVozilo == null)
                return NotFound();

            _context.Oglasvozilos.Remove(oglasVozilo);
            await _context.SaveChangesAsync();

            return Ok(oglasVozilo);
        }
    }
}
