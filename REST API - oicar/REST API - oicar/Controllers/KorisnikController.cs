using CARSHARE_WEBAPP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using System;
using System.Collections;
using System.Security.Claims; 

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikController : ControllerBase
    {
        private readonly CarshareContext _context;
        private readonly IConfiguration _configuration;

        public KorisnikController(CarshareContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<KorisnikUpdateDTO>> Update(int id, [FromBody] KorisnikUpdateDTO korisnikDto)
        {
            try
            {
                var korisnik = await _context.Korisniks.FindAsync(id);

                if (korisnik == null)
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");

                korisnik.Ime = korisnikDto.Ime;
                korisnik.Prezime = korisnikDto.Prezime;
                korisnik.Email = korisnikDto.Email;
                korisnik.Telefon = korisnikDto.Telefon;
                korisnik.Datumrodjenja = korisnikDto.DatumRodjenja;
                korisnik.Username = korisnikDto.Username;

                _context.Korisniks.Update(korisnik);
                await _context.SaveChangesAsync();

                return Ok(korisnikDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KorisnikDTO>>> GetAll()
        {
            return await _context.Korisniks
                .Include(x => x.Uloga)
                .Select(k => new KorisnikDTO
                {
                    IDKorisnik = k.Idkorisnik,
                    Ime = k.Ime,
                    Prezime = k.Prezime,
                    DatumRodjenja = k.Datumrodjenja,
                    Email = k.Email,
                    Username = k.Username,
                    Pwdhash = k.Pwdhash,
                    Pwdsalt = k.Pwdsalt,
                    Telefon = k.Telefon,
                    UlogaId = k.Ulogaid,
                    Uloga = k.Uloga,
                    Isconfirmed = k.Isconfirmed,
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Korisnik>> GetById(int id)
        {
            var korisnik = await _context.Korisniks.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }
            return korisnik;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<RegistracijaVozacDTO>> RegistracijaVozac([FromBody] RegistracijaVozacDTO registracijaVozacDTO)
        {
            try
            {
                var trimmedUsername = registracijaVozacDTO.Username.Trim();
                if (_context.Korisniks.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registracijaVozacDTO.Password, b64salt);

                var user = new Korisnik
                {
                    Username = registracijaVozacDTO.Username,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = registracijaVozacDTO.Ime,
                    Prezime = registracijaVozacDTO.Prezime,
                    Email = registracijaVozacDTO.Email,
                    Telefon = registracijaVozacDTO.Telefon,
                    Datumrodjenja = registracijaVozacDTO.Datumrodjenja,
                    Isconfirmed = true,

                };

                _context.Korisniks.Add(user);
                await _context.SaveChangesAsync();

                registracijaVozacDTO.Id = user.Idkorisnik;

                return Ok(registracijaVozacDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<RegistracijaVozacDTO>> Registracija([FromBody] RegistracijaVozacDTO registracijaVozacDTO)
        {
            try
            {
                var trimmedUsername = registracijaVozacDTO.Username.Trim();
                if (_context.Korisniks.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registracijaVozacDTO.Password, b64salt);

                var user = new Korisnik
                {
                    Username = registracijaVozacDTO.Username,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = registracijaVozacDTO.Ime,
                    Prezime = registracijaVozacDTO.Prezime,
                    Email = registracijaVozacDTO.Email,
                    Telefon = registracijaVozacDTO.Telefon,
                    Datumrodjenja = registracijaVozacDTO.Datumrodjenja,
                
                };

                user.Imagevozacka = await SaveImageFromBase64Async(registracijaVozacDTO.Vozacka, "Vozacka");
                user.Imageosobna = await SaveImageFromBase64Async(registracijaVozacDTO.Osobna, "Osobna");
                user.Imagelice = await SaveImageFromBase64Async(registracijaVozacDTO.Selfie, "Selfie");

                _context.Korisniks.Add(user);
                await _context.SaveChangesAsync();

                registracijaVozacDTO.Id = user.Idkorisnik;

                return Ok(registracijaVozacDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<Image> SaveImageFromBase64Async(string base64Image, string name)
        {
            if (string.IsNullOrEmpty(base64Image))
                return null;

            byte[] imageData = Convert.FromBase64String(base64Image);

            var image = new Image
            {
                Name = name,
                Content = imageData
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<RegistracijaPutnikDTO>> RegistracijaPutnik([FromBody] RegistracijaPutnikDTO registracijaPutnikDTO)
        {
            try
            {
                var trimmedUsername = registracijaPutnikDTO.Username.Trim();
                if (_context.Korisniks.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registracijaPutnikDTO.Password, b64salt);

                var user = new Korisnik
                {
                    Username = registracijaPutnikDTO.Username,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = registracijaPutnikDTO.Ime,
                    Prezime = registracijaPutnikDTO.Prezime,
                    Email = registracijaPutnikDTO.Email,
                    Telefon = registracijaPutnikDTO.Telefon,
                    Datumrodjenja = registracijaPutnikDTO.Datumrodjenja,
                    Ulogaid = 1,
                    Isconfirmed = true,
                };

                user.Imageosobna = await SaveImageFromBase64Async(registracijaPutnikDTO.Osobna, "Osobna");
                user.Imagelice = await SaveImageFromBase64Async(registracijaPutnikDTO.Selfie, "Selfie");

                _context.Korisniks.Add(user);
                await _context.SaveChangesAsync();

                registracijaPutnikDTO.Id = user.Idkorisnik;

                return Ok(registracijaPutnikDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public ActionResult Login(KorisnikLoginDTO korisnikLoginDTO)
        {
            try
            {
                var genericLoginFail = JsonConvert.SerializeObject("Incorrect username or password");

                var existingUser = _context.Korisniks.FirstOrDefault(x => x.Username == korisnikLoginDTO.Username);
                if (existingUser == null)
                {
                    return Unauthorized(genericLoginFail);
                }

                var b64hash = PasswordHashProvider.GetHash(korisnikLoginDTO.Password, existingUser.Pwdsalt);

                if (b64hash != existingUser.Pwdhash)
                {
                    return Unauthorized(genericLoginFail);
                }

                var secureKey = _configuration["JWT:SecureKey"];

                var serializedToken =
                    JwtTokenProvider.CreateToken(
                        secureKey,
                        120,
                        korisnikLoginDTO.Username);

                return Ok(JsonConvert.SerializeObject(serializedToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("potvrdi")]
        public async Task<IActionResult> PotvrdiIliOdbijKorisnika([FromBody] PotvrdaKorisnikDTO potvrdaKorisnikDTO)
        {
            var korisnik = await _context.Korisniks.FindAsync(potvrdaKorisnikDTO.Id);

            if (korisnik == null)
                return NotFound("Korisnik nije pronađen.");

            korisnik.Isconfirmed = potvrdaKorisnikDTO.IsConfirmed; 

            _context.Entry(korisnik).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var status = (bool)korisnik.Isconfirmed ? "potvrđen" : "odbijen";
            return Ok($"Korisnik je uspješno {status}.");
        }

        [HttpPost("[action]")]
        public ActionResult ChangePassword(KorisnikPromjenaLozinkeDTO changePasswordDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(changePasswordDto.Username) ||
                    string.IsNullOrWhiteSpace(changePasswordDto.OldPassword) ||
                    string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
                {
                    return BadRequest("There is no input");
                }

                var existingUser = _context.Korisniks
                    .FirstOrDefault(x => x.Username == changePasswordDto.Username);
                if (existingUser == null)
                {
                    return BadRequest("User does not exist");
                }

                var currentHash = PasswordHashProvider
                    .GetHash(changePasswordDto.OldPassword, existingUser.Pwdsalt);
                if (currentHash != existingUser.Pwdhash)
                {
                    return BadRequest("Old password is incorrect");
                }

                var newSalt = PasswordHashProvider.GetSalt();
                var newHash = PasswordHashProvider.GetHash(changePasswordDto.NewPassword, newSalt);

                existingUser.Pwdhash = newHash;
                existingUser.Pwdsalt = newSalt;

                _context.Update(existingUser);
                _context.SaveChanges();

                return Ok("Password was changed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
