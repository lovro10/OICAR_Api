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

        [HttpPost("[action]")]
        public async Task<IActionResult> PridruziSeVoznji([FromBody] KorisnikVoznjaDTO korisnikVoznjaDTO)
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
    }
}
