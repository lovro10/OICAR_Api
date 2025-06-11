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
    public class OglasVoziloController : ControllerBase
    { 
        private readonly CarshareContext _context;

        public OglasVoziloController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<OglasVoziloDTO>>> GetReservedDates(int id, int userId)
        { 
            var reservations = await _context.Korisnikvozilos
                .Include(kv => kv.Oglasvozilo) 
                .Where(kv => kv.Oglasvoziloid == id && kv.Korisnikid == userId)
                .ToListAsync();

            var result = new List<OglasVoziloDTO>(); 

            foreach (var res in reservations) 
            { 
                var reservedDates = new List<string>();
                var start = res.DatumPocetkaRezervacije.Date;
                var end = res.DatumZavrsetkaRezervacije.Date;

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    reservedDates.Add(date.ToString("yyyy-MM-dd"));
                }

                var carAd = res.Oglasvozilo; 

                result.Add(new OglasVoziloDTO 
                {     
                    IdOglasVozilo = carAd.Idoglasvozilo,  
                    Marka = carAd.Vozilo.Marka, 
                    Model = carAd.Vozilo.Model, 
                    Registracija = carAd.Vozilo.Registracija, 
                    Username = carAd.Vozilo.Vozac.Username, 
                    Ime = carAd.Vozilo.Vozac.Ime, 
                    Prezime = carAd.Vozilo.Vozac.Prezime,  
                    ReservedDates = reservedDates 
                });
            }

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> CreateReservation([FromBody] VehicleReservationDTO model)
        {
            var normalizedStart = model.DatumPocetkaRezervacije.Date.AddHours(12);
            var normalizedEnd = model.DatumZavrsetkaRezervacije.Date.AddHours(12);

            var existingReservations = await _context.Korisnikvozilos
                .Where(kv => kv.Oglasvoziloid == model.OglasVoziloId)
                .Where(kv =>
                    kv.DatumPocetkaRezervacije < normalizedEnd &&
                    kv.DatumZavrsetkaRezervacije > normalizedStart)
                .ToListAsync();

            if (existingReservations.Any())
            {
                return BadRequest("The selected dates overlap with an existing reservation.");
            }

            var reservation = new Korisnikvozilo
            {
                Korisnikid = model.KorisnikId,
                Oglasvoziloid = model.OglasVoziloId,
                DatumPocetkaRezervacije = normalizedStart,
                DatumZavrsetkaRezervacije = normalizedEnd
            };

            _context.Korisnikvozilos.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(reservation);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoziloDTO>>> GetAll()
        { 
            var oglasiVozila = await _context.Oglasvozilos
                .Include(o => o.Vozilo)
                .OrderByDescending(o => o.Idoglasvozilo) 
                .Select(o => new OglasVoziloDTO 
                { 
                    IdOglasVozilo = o.Idoglasvozilo,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    DatumPocetkaRezervacije = o.DatumPocetkaRezervacije,
                    DatumZavrsetkaRezervacije = o.DatumZavrsetkaRezervacije, 
                    KorisnikId = o.Vozilo.Vozacid, 
                    Username = o.Vozilo.Vozac.Username,
                    Ime = o.Vozilo.Vozac.Ime,
                    Prezime = o.Vozilo.Vozac.Prezime,
                    Email = o.Vozilo.Vozac.Email
                })
                .ToListAsync();

            return Ok(oglasiVozila);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoziloDTO>> GetOglasVoziloById(int id)
        {
            var oglasVozilo = await _context.Oglasvozilos
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
                DatumZavrsetkaRezervacije = o.DatumZavrsetkaRezervacije,
                KorisnikId = o.Vozilo.Vozacid,
                Username = o.Vozilo.Vozac.Username, 
                Ime = o.Vozilo.Vozac.Ime, 
                Prezime = o.Vozilo.Vozac.Prezime,
                Email = o.Vozilo.Vozac.Email
            }) 
            .FirstOrDefaultAsync();

            if (oglasVozilo == null)
                return NotFound();

            return Ok(oglasVozilo);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoziloDTO>>> GetAllByUser(int userId)
        {
            var oglasiVozila = await _context.Oglasvozilos
                .Where(x => x.Vozilo!.Vozacid == userId)
                .Include(o => o.Vozilo)
                .OrderByDescending(o => o.Idoglasvozilo)
                .Select(o => new OglasVoziloDTO
                {
                    IdOglasVozilo = o.Idoglasvozilo,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo!.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    DatumPocetkaRezervacije = o.DatumPocetkaRezervacije,
                    DatumZavrsetkaRezervacije = o.DatumZavrsetkaRezervacije,
                    KorisnikId = o.Vozilo.Vozacid,
                    Username = o.Vozilo.Vozac.Username,
                    Ime = o.Vozilo.Vozac.Ime,
                    Prezime = o.Vozilo.Vozac.Prezime,
                    Email = o.Vozilo.Vozac.Email
                })
                .ToListAsync();

            return Ok(oglasiVozila);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoziloDTO>>> GetRentedCars(int userId)
        {
            var rentedCars = await _context.Korisnikvozilos 
                .Where(x => x.Korisnikid == userId && x.Oglasvozilo!.Vozilo!.Vozac!.Idkorisnik != userId)
                .Include(o => o.Oglasvozilo)
                    .ThenInclude(o => o.Vozilo)
                .OrderByDescending(o => o.Oglasvozilo.Idoglasvozilo)
                .Select(o => new OglasVoziloDTO
                {
                    IdOglasVozilo = o.Oglasvozilo.Idoglasvozilo,
                    VoziloId = o.Oglasvozilo.Voziloid,
                    Marka = o.Oglasvozilo.Vozilo!.Marka,
                    Model = o.Oglasvozilo.Vozilo.Model,
                    Registracija = o.Oglasvozilo.Vozilo.Registracija,
                    DatumPocetkaRezervacije = o.DatumPocetkaRezervacije,
                    DatumZavrsetkaRezervacije = o.DatumZavrsetkaRezervacije,
                    KorisnikId = o.Oglasvozilo.Vozilo.Vozacid,
                    Username = o.Oglasvozilo.Vozilo.Vozac.Username,
                    Ime = o.Oglasvozilo.Vozilo.Vozac.Ime,
                    Prezime = o.Oglasvozilo.Vozilo.Vozac.Prezime 
                })
                .ToListAsync();

            return Ok(rentedCars);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoznjaDTO>> DetaljiOglasaVozila(int id)
        {
            var oglasVozilo = await _context.Oglasvozilos
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
                DatumZavrsetkaRezervacije = o.DatumZavrsetkaRezervacije,
                Username = o.Vozilo.Vozac.Username,
                Ime = o.Vozilo.Vozac.Ime,
                Prezime = o.Vozilo.Vozac.Prezime,
                Email = o.Vozilo.Vozac.Email
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

            var existingAd = await _context.Oglasvozilos
                .AnyAsync(o => o.Voziloid == oglasVoziloDTO.VoziloId);

            if (existingAd)
            {
                return BadRequest($"An ad already exists for the selected vehicle (ID: {oglasVoziloDTO.VoziloId}).");
            }

            var noviOglasVozilo = new Oglasvozilo
            {
                Voziloid = oglasVoziloDTO.VoziloId,
                DatumPocetkaRezervacije = oglasVoziloDTO.DatumPocetkaRezervacije, 
                DatumZavrsetkaRezervacije = oglasVoziloDTO.DatumZavrsetkaRezervacije 
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
            
            _context.Oglasvozilos.Update(oglasVozilo); 

            await _context.SaveChangesAsync();

            oglasVoziloDTO.IdOglasVozilo = oglasVozilo.Idoglasvozilo;
            
            return Ok(oglasVoziloDTO);
        } 

        [HttpDelete("[action]")]
        public async Task<IActionResult> ObrisiOglasVozilo(int id)
        { 
            var oglasVozilo = await _context.Oglasvozilos 
                .Include(o => o.Vozilo) 
                .FirstOrDefaultAsync(o => o.Idoglasvozilo == id);

            if (oglasVozilo == null)
                return NotFound();

            _context.Oglasvozilos.Remove(oglasVozilo);
            await _context.SaveChangesAsync();

            return Ok(oglasVozilo);
        }
    }
}
