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
    [Authorize] // JWT token
    public class OglasVoznjaController : ControllerBase
    {
        private readonly CarshareContext _context;

        public OglasVoznjaController(CarshareContext context)
        {
            _context = context;
        }

        // POST: api/OglasVoznja/kreiraj
        [HttpPost("kreiraj")]
        public async Task<IActionResult> KreirajOglasVoznja([FromBody] OglasVoznjaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DatumIVrijemePolaska < DateTime.UtcNow)
                return BadRequest("Datum i vrijeme polaska mora biti u budućnosti.");
            if (dto.DatumIVrijemeDolaska <= dto.DatumIVrijemePolaska)
                return BadRequest("Datum i vrijeme dolaska mora biti nakon polaska.");

            var driverIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverIdString))
                return Unauthorized("Niste autorizirani.");
            int driverId = int.Parse(driverIdString);

            bool ownsVehicle = await _context.Oglasvozilos
                .AnyAsync(ov => ov.Korisnikid == driverId && ov.Voziloid == dto.VoziloId);

            if (!ownsVehicle)
                return BadRequest("Odabrano vozilo ne postoji ili ne pripada vozaču.");

            var lokacija = new Lokacija
            {
                Polaziste = dto.LokacijaPolaziste,
                Odrediste = dto.LokacijaOdrediste
            };
            _context.Lokacijas.Add(lokacija);
            await _context.SaveChangesAsync();

            var oglas = new Oglasvoznja
            {
                Voziloid = dto.VoziloId,
                DatumIVrijemePolaska = DateTime.SpecifyKind(dto.DatumIVrijemePolaska, DateTimeKind.Unspecified),
                DatumIVrijemeDolaska = DateTime.SpecifyKind(dto.DatumIVrijemeDolaska, DateTimeKind.Unspecified),
                BrojPutnika = dto.BrojPutnika,
                Lokacijaid = lokacija.Idlokacija,
                //Statusvoznjeid =  0,  //TODO, ne radi kako treba
                Troskoviid = null   // TODO, kasnije dodati
            };


            _context.Oglasvoznjas.Add(oglas);
            await _context.SaveChangesAsync();

            return Ok(new { poruka = "Oglas vožnja uspješno kreiran.", id = oglas.Idoglasvoznja });
        }
    }
}
