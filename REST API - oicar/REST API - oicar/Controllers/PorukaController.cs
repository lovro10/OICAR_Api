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

        [HttpGet("[action]")] 
        public async Task<ActionResult<IEnumerable<PorukaGetDTO>>> GetMessagesForRide(int korisnikVoznjaId)
        {
            var messages = await _context.Porukas
                .Where(x => x.Korisnikvoznjaid == korisnikVoznjaId)
                .OrderBy(x => x.Idporuka)
                .Select(x => new PorukaGetDTO
                { 
                    Idporuka = x.Idporuka,
                    Content = x.Content,
                    KorisnikVoznjaId = x.Korisnikvoznjaid ?? 0,
                    PutnikId = x.Putnikid,
                    VozacId = x.Vozacid,
                    SenderName = x.Putnik != null ? x.Putnik.Username :
                                 x.Vozac != null ? x.Vozac.Username : "Unknown user"
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> SendMessageForRide([FromBody] PorukaSendDTO porukaSendDTO)
        {
            if (porukaSendDTO.KorisnikVoznjaId == 0 || string.IsNullOrWhiteSpace(porukaSendDTO.Content))
                return BadRequest("KorisnikVoznjaId i sadržaj poruke su obavezni.");

            if (porukaSendDTO.PutnikId == null && porukaSendDTO.VozacId == null)
                return BadRequest("Pošiljatelj mora biti definiran (putnikId ili vozacId).");

            if (porukaSendDTO.PutnikId != null && porukaSendDTO.VozacId != null)
                return BadRequest("Poruku može poslati samo jedan korisnik (putnik ILI vozač, ne oba).");

            var korisnikVoznja = await _context.Korisnikvoznjas
                .Include(kv => kv.Korisnik)
                .Include(kv => kv.Oglasvoznja)
                .FirstOrDefaultAsync(kv => kv.Idkorisnikvoznja == porukaSendDTO.KorisnikVoznjaId);

            if (korisnikVoznja == null)
                return NotFound("Voznja ne postoji.");

            var actualPutnikId = korisnikVoznja.Korisnikid;
            var actualVozacId = korisnikVoznja.Oglasvoznja?.Vozilo?.Vozacid;

            if (porukaSendDTO.PutnikId != null && porukaSendDTO.PutnikId != actualPutnikId)
                return Forbid("Pošiljatelj nije putnik u ovoj vožnji.");

            if (porukaSendDTO.VozacId != null && porukaSendDTO.VozacId != actualVozacId)
                return Forbid("Pošiljatelj nije vozač u ovoj vožnji.");

            var newMessage = new Poruka
            {
                Korisnikvoznjaid = porukaSendDTO.KorisnikVoznjaId,
                Putnikid = porukaSendDTO.PutnikId,
                Vozacid = porukaSendDTO.VozacId,
                Content = porukaSendDTO.Content
            };

            _context.Porukas.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent" });
        }
    }
}
