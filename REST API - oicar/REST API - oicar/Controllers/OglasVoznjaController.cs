using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OglasVoznjaController : ControllerBase
    {
        private readonly CarshareContext _context;

        public OglasVoznjaController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoznjaDTO>>> GetAll()
        {
            var oglasiVoznje = await _context.Oglasvoznjas
                .Include(o => o.Troskovi)
                .Include(o => o.Lokacija)
                .Include(o => o.Vozilo)
                .Include(o => o.Statusvoznje)
                .Include(x => x.Korisnikvoznjas) 
                .OrderByDescending(o => o.Idoglasvoznja)
                .Select(o => new OglasVoznjaDTO 
                {  
                    IdOglasVoznja = o.Idoglasvoznja, 
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    Username = o.Vozilo.Vozac.Username, 
                    Ime = o.Vozilo.Vozac.Ime, 
                    Prezime = o.Vozilo.Vozac.Prezime,
                    DatumIVrijemePolaska = o.DatumIVrijemePolaska,
                    DatumIVrijemeDolaska = o.DatumIVrijemeDolaska,
                    BrojPutnika = o.BrojPutnika,
                    TroskoviId = o.Troskoviid,
                    LokacijaId = o.Lokacijaid,
                    StatusVoznjeId = o.Statusvoznjeid,
                    Cestarina = o.Troskovi.Cestarina,
                    Gorivo = o.Troskovi.Gorivo,
                    Polaziste = o.Lokacija.Polaziste,
                    Odrediste = o.Lokacija.Odrediste,
                    StatusVoznjeNaziv = o.Statusvoznje.Naziv,
                    CijenaPoPutniku = o.BrojPutnika > 0
                        ? (o.Troskovi.Cestarina + o.Troskovi.Gorivo) / o.BrojPutnika
                        : 0 
                }) 
                .ToListAsync();

            return Ok(oglasiVoznje);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<OglasVoznjaDTO>>> GetAllByUser(int userId) 
        { 
            var oglasiVoznje = await _context.Oglasvoznjas
                .Where(x => x.Vozilo.Vozacid == userId) 
                .Include(o => o.Troskovi)
                .Include(o => o.Lokacija)
                .Include(o => o.Vozilo)
                .Include(o => o.Statusvoznje)
                .OrderByDescending(o => o.Idoglasvoznja)
                .Select(o => new OglasVoznjaDTO
                {
                    IdOglasVoznja = o.Idoglasvoznja,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    Username = o.Vozilo.Vozac.Username,
                    Ime = o.Vozilo.Vozac.Ime,
                    Prezime = o.Vozilo.Vozac.Prezime,
                    DatumIVrijemePolaska = o.DatumIVrijemePolaska,
                    DatumIVrijemeDolaska = o.DatumIVrijemeDolaska,
                    BrojPutnika = o.BrojPutnika,
                    TroskoviId = o.Troskoviid,
                    LokacijaId = o.Lokacijaid,
                    StatusVoznjeId = o.Statusvoznjeid,
                    Cestarina = o.Troskovi.Cestarina,
                    Gorivo = o.Troskovi.Gorivo,
                    Polaziste = o.Lokacija.Polaziste,
                    Odrediste = o.Lokacija.Odrediste,
                    StatusVoznjeNaziv = o.Statusvoznje.Naziv,
                    CijenaPoPutniku = o.BrojPutnika > 0
                        ? (o.Troskovi.Cestarina + o.Troskovi.Gorivo) / o.BrojPutnika
                        : 0
                })
                .ToListAsync();

            return Ok(oglasiVoznje);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoznjaDTO>> GetById(int id)
        {
            var oglasVoznja = await _context.Oglasvoznjas
                .Include(o => o.Troskovi)
                .Include(o => o.Lokacija)
                .Include(o => o.Statusvoznje)
                .Where(o => o.Idoglasvoznja == id)
                .Select(o => new OglasVoznjaDTO
                {
                    IdOglasVoznja = o.Idoglasvoznja,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    Username = o.Vozilo.Vozac.Username,
                    Ime = o.Vozilo.Vozac.Ime,
                    Prezime = o.Vozilo.Vozac.Prezime,
                    DatumIVrijemePolaska = o.DatumIVrijemePolaska,
                    DatumIVrijemeDolaska = o.DatumIVrijemeDolaska,
                    BrojPutnika = o.BrojPutnika,
                    TroskoviId = o.Troskoviid,
                    LokacijaId = o.Lokacijaid,
                    StatusVoznjeId = o.Statusvoznjeid,
                    Cestarina = o.Troskovi.Cestarina,
                    Gorivo = o.Troskovi.Gorivo,
                    Polaziste = o.Lokacija.Polaziste,
                    Odrediste = o.Lokacija.Odrediste,
                    StatusVoznjeNaziv = o.Statusvoznje.Naziv,
                    CijenaPoPutniku = o.BrojPutnika > 0
                        ? (o.Troskovi.Cestarina + o.Troskovi.Gorivo) / o.BrojPutnika
                        : 0
                })
                .FirstOrDefaultAsync();

            if (oglasVoznja == null)
                return NotFound();

            return Ok(oglasVoznja);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<OglasVoznjaDTO>> GetDataForAd(int id)
        { 
            var oglasVoznja = await _context.Oglasvoznjas
                .Include(o => o.Vozilo)
                    .ThenInclude(v => v.Vozac) 
                .Include(o => o.Troskovi) 
                .Include(o => o.Lokacija) 
                .Include(o => o.Statusvoznje)
                .Where(o => o.Idoglasvoznja == id)
                .Select(o => new OglasVoznjaDTO
                {  
                    IdOglasVoznja = o.Idoglasvoznja,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    KorisnikId = o.Vozilo.Vozacid,  
                    Username = o.Vozilo.Vozac.Username, 
                    Ime = o.Vozilo.Vozac.Ime,
                    Prezime = o.Vozilo.Vozac.Prezime,
                    DatumIVrijemePolaska = o.DatumIVrijemePolaska,
                    DatumIVrijemeDolaska = o.DatumIVrijemeDolaska,
                    BrojPutnika = o.BrojPutnika,
                    TroskoviId = o.Troskoviid,
                    LokacijaId = o.Lokacijaid,
                    Cestarina = o.Troskovi.Cestarina,
                    Gorivo = o.Troskovi.Gorivo,
                    Polaziste = o.Lokacija.Polaziste,
                    Odrediste = o.Lokacija.Odrediste,
                    CijenaPoPutniku = o.BrojPutnika > 0
                        ? (o.Troskovi.Cestarina + o.Troskovi.Gorivo) / o.BrojPutnika
                        : 0
                })
                .FirstOrDefaultAsync();

            if (oglasVoznja == null)
                return NotFound();

            return Ok(oglasVoznja);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<OglasVoznjaDTO>> DetaljiOglasaVoznje(int id)
        {
            var oglasVoznja = await _context.Oglasvoznjas
                .Include(o => o.Troskovi)
                .Include(o => o.Lokacija)
                .Include(o => o.Statusvoznje)
                .Where(o => o.Idoglasvoznja == id)
                .Select(o => new OglasVoznjaDTO
                {
                    IdOglasVoznja = o.Idoglasvoznja,
                    VoziloId = o.Voziloid,
                    Marka = o.Vozilo.Marka,
                    Model = o.Vozilo.Model,
                    Registracija = o.Vozilo.Registracija,
                    DatumIVrijemePolaska = o.DatumIVrijemePolaska,
                    DatumIVrijemeDolaska = o.DatumIVrijemeDolaska,
                    BrojPutnika = o.BrojPutnika,
                    TroskoviId = o.Troskoviid,
                    LokacijaId = o.Lokacijaid,
                    StatusVoznjeId = o.Statusvoznjeid,
                    Cestarina = o.Troskovi.Cestarina,
                    Gorivo = o.Troskovi.Gorivo,
                    Polaziste = o.Lokacija.Polaziste,
                    Odrediste = o.Lokacija.Odrediste,
                    StatusVoznjeNaziv = o.Statusvoznje.Naziv
                })
                .FirstOrDefaultAsync();

            if (oglasVoznja == null)
                return NotFound();

            return Ok(oglasVoznja);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> KreirajOglasVoznje([FromBody] OglasVoznjaDTO oglasVoznjaDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noviTroskovi = new Troskovi
            {
                Cestarina = oglasVoznjaDTO.Cestarina,  
                Gorivo = oglasVoznjaDTO.Gorivo 
            }; 

            _context.Troskovis.Add(noviTroskovi);
            await _context.SaveChangesAsync();

            var novaLokacija = new Lokacija
            {
                Polaziste = oglasVoznjaDTO.Polaziste,
                Odrediste = oglasVoznjaDTO.Odrediste
            };

            _context.Lokacijas.Add(novaLokacija);
            await _context.SaveChangesAsync();

            var novaVoznja = new Oglasvoznja
            {
                Voziloid = oglasVoznjaDTO.VoziloId,
                DatumIVrijemePolaska = oglasVoznjaDTO.DatumIVrijemePolaska,
                DatumIVrijemeDolaska = oglasVoznjaDTO.DatumIVrijemeDolaska,
                Troskoviid = noviTroskovi.Idtroskovi,
                Lokacijaid = novaLokacija.Idlokacija,
                BrojPutnika = oglasVoznjaDTO.BrojPutnika,
                Statusvoznjeid = 1 
            };

            _context.Oglasvoznjas.Add(novaVoznja);
            await _context.SaveChangesAsync();

            oglasVoznjaDTO.IdOglasVoznja = novaVoznja.Idoglasvoznja;
            oglasVoznjaDTO.TroskoviId = noviTroskovi.Idtroskovi;
            oglasVoznjaDTO.LokacijaId = novaLokacija.Idlokacija;
            
            var korisnikVoznja = new Korisnikvoznja
            {
                Korisnikid = oglasVoznjaDTO.KorisnikId,
                Oglasvoznjaid = novaVoznja.Idoglasvoznja,
                Lokacijavozac = oglasVoznjaDTO.Polaziste,
                Lokacijaputnik = oglasVoznjaDTO.Polaziste
            };

            _context.Korisnikvoznjas.Add(korisnikVoznja);
            await _context.SaveChangesAsync();

            return Ok(oglasVoznjaDTO);
        } 
        
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> AzurirajOglasVoznje(int id, [FromBody] OglasVoznjaDTO oglasVoznjaDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oglasVoznja = await _context.Oglasvoznjas
                .FirstOrDefaultAsync(o => o.Idoglasvoznja == id);

            if (oglasVoznja == null)
                return NotFound();

            var troskovi = await _context.Troskovis
                .FirstOrDefaultAsync(t => t.Idtroskovi == oglasVoznja.Troskoviid);

            if (troskovi != null)
            {
                troskovi.Cestarina = oglasVoznjaDTO.Cestarina ?? troskovi.Cestarina;
                troskovi.Gorivo = oglasVoznjaDTO.Gorivo ?? troskovi.Gorivo;
                _context.Troskovis.Update(troskovi);
            }
            else
            {
                troskovi = new Troskovi
                {
                    Cestarina = oglasVoznjaDTO.Cestarina ?? 0,
                    Gorivo = oglasVoznjaDTO.Gorivo ?? 0
                };
                _context.Troskovis.Add(troskovi);
            }

            var lokacija = await _context.Lokacijas
                .FirstOrDefaultAsync(l => l.Idlokacija == oglasVoznja.Lokacijaid);

            if (lokacija != null)
            {
                lokacija.Polaziste = oglasVoznjaDTO.Polaziste ?? lokacija.Polaziste;
                lokacija.Odrediste = oglasVoznjaDTO.Odrediste ?? lokacija.Odrediste;
                _context.Lokacijas.Update(lokacija);
            }
            else
            {
                lokacija = new Lokacija
                {
                    Polaziste = oglasVoznjaDTO.Polaziste,
                    Odrediste = oglasVoznjaDTO.Odrediste
                };
                _context.Lokacijas.Add(lokacija);
            }

            var status = await _context.Statusvoznjes
                .FirstOrDefaultAsync(s => s.Idstatusvoznje == oglasVoznja.Statusvoznjeid);

            if (status != null)
            {
                status.Naziv = "Ažurirano";
                _context.Statusvoznjes.Update(status);
            }
            else
            {
                status = new Statusvoznje
                {
                    Naziv = "Ažurirano"
                };
                _context.Statusvoznjes.Add(status);
            }

            oglasVoznja.DatumIVrijemePolaska = oglasVoznjaDTO.DatumIVrijemePolaska;
            oglasVoznja.DatumIVrijemeDolaska = oglasVoznjaDTO.DatumIVrijemeDolaska;
            oglasVoznja.BrojPutnika = oglasVoznjaDTO.BrojPutnika;
            oglasVoznja.Voziloid = oglasVoznjaDTO.VoziloId;

            _context.Oglasvoznjas.Update(oglasVoznja);

            await _context.SaveChangesAsync();

            oglasVoznjaDTO.IdOglasVoznja = oglasVoznja.Idoglasvoznja;
            oglasVoznjaDTO.TroskoviId = troskovi.Idtroskovi;
            oglasVoznjaDTO.LokacijaId = lokacija.Idlokacija;
            oglasVoznjaDTO.StatusVoznjeId = status.Idstatusvoznje;

            return Ok(oglasVoznjaDTO);
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> ObrisiOglasVoznje(int id)
        {
            var oglasVoznja = await _context.Oglasvoznjas
                .Include(o => o.Troskovi)
                .Include(o => o.Lokacija)
                .Include(o => o.Statusvoznje)
                .FirstOrDefaultAsync(o => o.Idoglasvoznja == id);

            if (oglasVoznja == null)
                return NotFound();


            _context.Troskovis.Remove(oglasVoznja.Troskovi);
            _context.Lokacijas.Remove(oglasVoznja.Lokacija);
            _context.Statusvoznjes.Remove(oglasVoznja.Statusvoznje);

            _context.Oglasvoznjas.Remove(oglasVoznja);
            await _context.SaveChangesAsync();

            return Ok(oglasVoznja);
        }
    }
}
