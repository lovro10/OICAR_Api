using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikVoznjaController : Controller
    { 
        private readonly CarshareContext _context;

        public KorisnikVoznjaController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<KorisnikVoznjaDTO>>> GetAll() 
        { 
            var korisnikoveVoznje = await _context.Korisnikvoznjas 
                .Include(x => x.Korisnik) 
                .Include(x => x.Oglasvoznja) 
                .Include(x => x.Porukas) 
                .Select(x => new KorisnikVoznjaDTO 
                { 
                    IdKorisnikVoznja = x.Idkorisnikvoznja, 
                    KorisnikId = x.Korisnikid,  
                    OglasVoznjaId = x.Oglasvoznjaid, 
                    LokacijaPutnik = x.Lokacijaputnik, 
                    LokacijaVozac = x.Lokacijavozac
                }) 
                .ToListAsync(); 

            return Ok(korisnikoveVoznje);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> JoinRide([FromBody] KorisnikVoznjaDTO korisnikVoznjaDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var voznja = await _context.Oglasvoznjas
                .Include(o => o.Korisnikvoznjas) 
                .FirstOrDefaultAsync(o => o.Idoglasvoznja == korisnikVoznjaDTO.OglasVoznjaId);

            if (voznja == null)
                return NotFound("Vožnja nije pronađena.");

            bool jeVecPrijavljen = voznja.Korisnikvoznjas.Any(kv => kv.Korisnikid == korisnikVoznjaDTO.KorisnikId);
            if (jeVecPrijavljen)
                return BadRequest("Korisnik je već prijavljen na ovu vožnju.");

            if (voznja.Korisnikvoznjas.Count >= voznja.BrojPutnika)
                return BadRequest("Nema slobodnih mjesta u ovoj vožnji.");

            var novaPrijava = new Korisnikvoznja
            {
                Korisnikid = korisnikVoznjaDTO.KorisnikId,
                Oglasvoznjaid = korisnikVoznjaDTO.OglasVoznjaId,
                Lokacijaputnik = korisnikVoznjaDTO.LokacijaPutnik,
                Lokacijavozac = korisnikVoznjaDTO.LokacijaVozac
            };

            _context.Korisnikvoznjas.Add(novaPrijava);
            await _context.SaveChangesAsync();

            korisnikVoznjaDTO.IdKorisnikVoznja = novaPrijava.Idkorisnikvoznja;

            return Ok(korisnikVoznjaDTO); 
        }

        [HttpGet("[action]")] 
        public async Task<IActionResult> UserJoinedRide(int userId, int oglasVoznjaId)
        { 
            if (userId <= 0 || oglasVoznjaId <= 0)
                return BadRequest(false);

            bool isPassenger = await _context.Korisnikvoznjas
                .AnyAsync(kv => kv.Korisnikid == userId && kv.Oglasvoznjaid == oglasVoznjaId);

            if (isPassenger)
                return Ok(true);

            var ride = await _context.Oglasvoznjas
                .Include(o => o.Vozilo)
                .FirstOrDefaultAsync(o => o.Idoglasvoznja == oglasVoznjaId);

            if (ride != null && ride.Vozilo != null && ride.Vozilo.Vozacid == userId)
                return Ok(true);

            return Ok(false);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LeaveRide([FromBody] KorisnikVoznjaDTO korisnikVoznjaDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ride = await _context.Korisnikvoznjas
                .FirstOrDefaultAsync(kv =>
                    kv.Korisnikid == korisnikVoznjaDTO.KorisnikId &&
                    kv.Oglasvoznjaid == korisnikVoznjaDTO.OglasVoznjaId);

            if (ride == null)
                return NotFound("User did not enter");

            _context.Korisnikvoznjas.Remove(ride);
            await _context.SaveChangesAsync();

            return Ok("Successfully left the ride");
        }

        [HttpGet("[action]")] 
        public async Task<ActionResult<IEnumerable<VoznjaHistoryDTO>>> GetHistoryOfRides(int korisnikId)
        { 
            var history = await _context.Korisnikvoznjas
                .Where(x => x.Korisnikid == korisnikId &&
                             x.Oglasvoznja != null &&
                             x.Oglasvoznja.DatumIVrijemePolaska < DateTime.Now)
                .Include(x => x.Oglasvoznja)
                .Select(x => new VoznjaHistoryDTO
                {
                    Korisnikvoznjaid = x.Idkorisnikvoznja,
                    Oglasvoznjaid = x.Oglasvoznjaid,
                    DatumVoznje = x.Oglasvoznja!.DatumIVrijemePolaska,
                    Polaziste = x.Oglasvoznja!.Lokacija!.Polaziste,
                    Odrediste = x.Oglasvoznja.Lokacija.Odrediste,
                    Lokacijavozac = x.Lokacijavozac,
                    Lokacijaputnik = x.Lokacijaputnik
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}
