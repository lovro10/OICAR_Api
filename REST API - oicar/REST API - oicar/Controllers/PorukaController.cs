using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PorukaController : Controller
    { 
        private readonly CarshareContext _context;

        public PorukaController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]/{korisnikVoznjaId}")]
        public async Task<IActionResult> DohvatiPoruke(int korisnikVoznjaId)
        {
            var poruke = await _context.Porukas
                .Where(p => p.Korisnikvoznjaid == korisnikVoznjaId)
                .Include(p => p.Putnik)
                .Include(p => p.Vozac)
                .ToListAsync();

            if (poruke == null || !poruke.Any())
                return NotFound("Nema poruka za ovu vožnju.");

            var porukeDTO = poruke.Select(p => new
            {
                p.Idporuka,
                p.Content,
                p.Putnikid,
                p.Vozacid,
                PutnikIme = p.Putnik.Ime,
                VozacIme = p.Vozac.Ime 
            }).ToList(); 

            return Ok(porukeDTO);
        } 

        [HttpPost("[action]")]
        public async Task<IActionResult> PosaljitePoruku([FromBody] PorukaDTO porukaDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var korisnikVoznja = await _context.Korisnikvoznjas
                .FirstOrDefaultAsync(kv => kv.Idkorisnikvoznja == porukaDTO.KorisnikVoznjaId);

            if (korisnikVoznja == null)
                return NotFound("Korisnička vožnja nije pronađena.");

            var korisnik = await _context.Korisniks 
                .FirstOrDefaultAsync(k => k.Idkorisnik == porukaDTO.PutnikId || k.Idkorisnik == porukaDTO.VozacId);

            if (korisnik == null)
                return NotFound("Korisnik nije pronađen.");

            var novaPoruka = new Poruka
            {
                Korisnikvoznjaid = porukaDTO.KorisnikVoznjaId,
                Putnikid = porukaDTO.PutnikId,
                Vozacid = porukaDTO.VozacId,
                Content = porukaDTO.Content,
            };

            _context.Porukas.Add(novaPoruka);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Poruka uspješno poslana." });
        }
    }
}
