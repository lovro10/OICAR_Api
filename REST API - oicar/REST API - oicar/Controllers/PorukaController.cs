using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using REST_API___oicar.Security;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PorukaController : Controller
    { 
        private readonly CarshareContext _context;

        private readonly AesEncryptionService _encryptionService;

        public PorukaController(CarshareContext context, AesEncryptionService encryptionService) 
        { 
            _context = context; 
            _encryptionService = encryptionService; 
        } 

        [HttpGet("[action]")] 
        public async Task<ActionResult<IEnumerable<PorukaGetDTO>>> GetMessagesForRide(int oglasVoznjaId) 
        { 
            var messages = await _context.Porukas
                .Where(x => x.Oglasvoznjaid == oglasVoznjaId)
                .OrderBy(x => x.Idporuka)
                .Select(x => new PorukaGetDTO
                { 
                    Idporuka = x.Idporuka,
                    Content = x.Content,
                    OglasVoznjaId = x.Oglasvoznjaid ?? 0,
                    PutnikId = x.Putnikid,
                    VozacId = x.Vozacid,
                    SenderName = x.Putnik != null ? _encryptionService.Decrypt(x.Putnik.Username) :
                                 x.Vozac != null ? _encryptionService.Decrypt(x.Vozac.Username) : "Unknown user" 
                }) 
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessageForRide([FromBody] PorukaSendDTO porukaSendDTO)
        {
            if (porukaSendDTO.OglasVoznjaId == 0 || string.IsNullOrWhiteSpace(porukaSendDTO.Content))
                return BadRequest("OglasVoznjaId i sadržaj poruke su obavezni.");

            if (porukaSendDTO.PutnikId == null && porukaSendDTO.VozacId == null)
                return BadRequest("Pošiljatelj mora biti definiran (putnikId ili vozacId).");

            if (porukaSendDTO.PutnikId != null && porukaSendDTO.VozacId != null)
                return BadRequest("Poruku može poslati samo jedan korisnik (putnik ILI vozač, ne oba).");

            int? korisnikId = porukaSendDTO.PutnikId ?? porukaSendDTO.VozacId;

            var korisnikVoznja = await _context.Korisnikvoznjas
                .Where(kv => kv.Oglasvoznjaid == porukaSendDTO.OglasVoznjaId && kv.Korisnikid == korisnikId)
                .FirstOrDefaultAsync();

            if (korisnikVoznja == null)
                return Forbid("Pošiljatelj nije sudionik ove vožnje.");

            var newMessage = new Poruka
            {
                Oglasvoznjaid = korisnikVoznja.Oglasvoznjaid,
                Putnikid = porukaSendDTO.PutnikId,
                Vozacid = porukaSendDTO.VozacId,
                Content = porukaSendDTO.Content 
            };

            _context.Porukas.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(newMessage);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<PorukaGetDTO>>> GetMessagesForVehicle(int korisnikVoziloId) 
        { 
            var messages = await _context.Porukas
                .Where(x => x.Oglasvoznjaid == korisnikVoziloId) 
                .OrderBy(x => x.Idporuka) 
                .Select(x => new PorukaGetDTO
                { 
                    Idporuka = x.Idporuka,
                    Content = x.Content,
                    OglasVoznjaId = x.Oglasvoznjaid ?? 0,
                    PutnikId = x.Putnikid,
                    VozacId = x.Vozacid,
                    SenderName = x.Putnik != null ? x.Putnik.Username :
                                 x.Vozac != null ? x.Vozac.Username : "Unknown user"
                }) 
                .ToListAsync();

            return Ok(messages);
        } 

        [HttpPost("[action]")]
        public async Task<ActionResult> SendMessageForVehicle([FromBody] PorukaSendDTO porukaSendDTO)
        {
            if (porukaSendDTO.OglasVoznjaId == 0 || string.IsNullOrWhiteSpace(porukaSendDTO.Content))
                return BadRequest("KorisnikVoznjaId i sadržaj poruke su obavezni.");

            if (porukaSendDTO.PutnikId == null && porukaSendDTO.VozacId == null)
                return BadRequest("Pošiljatelj mora biti definiran (putnikId ili vozacId).");

            if (porukaSendDTO.PutnikId != null && porukaSendDTO.VozacId != null)
                return BadRequest("Poruku može poslati samo jedan korisnik (putnik ILI vozač, ne oba).");

            var korisnikVoznja = await _context.Korisnikvoznjas
                .Include(kv => kv.Korisnik)
                .Include(kv => kv.Oglasvoznja)
                .FirstOrDefaultAsync(kv => kv.Idkorisnikvoznja == porukaSendDTO.OglasVoznjaId);

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
                Oglasvoznjaid = porukaSendDTO.OglasVoznjaId,
                Putnikid = porukaSendDTO.PutnikId,
                Vozacid = porukaSendDTO.VozacId,
                Content = porukaSendDTO.Content
            };

            _context.Porukas.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(newMessage);
        }
    }
}
