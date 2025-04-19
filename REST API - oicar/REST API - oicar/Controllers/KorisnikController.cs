using CARSHARE_WEBAPP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
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
        public async Task<IActionResult> Update(int id, [FromBody] KorisnikDTO korisnikDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingKorisnik = _context.Korisniks.FirstOrDefault(k => k.Idkorisnik == id);
            if (existingKorisnik == null)
            {
                return NotFound($"Korisnik(id={id}) was not found.");
            }

            existingKorisnik.Ime = korisnikDto.Ime;
            existingKorisnik.Prezime = korisnikDto.Prezime;
            existingKorisnik.Email = korisnikDto.Email;
            existingKorisnik.Username = korisnikDto.Username;
            existingKorisnik.Telefon = korisnikDto.Telefon;
            existingKorisnik.Datumrodjenja = korisnikDto.DatumRodjenja;
            _context.SaveChanges();


            return Ok(new KorisnikDTO
            {

                Ime = korisnikDto.Ime,
                Prezime = korisnikDto.Prezime,
                Email = korisnikDto.Email,
                Username = korisnikDto.Username,
                Telefon = korisnikDto.Telefon,
                DatumRodjenja = korisnikDto.DatumRodjenja,

            });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Korisnik>>> GetAll()
        {
            return await _context.Korisniks
                .Select(k => new Korisnik
                {
                    Idkorisnik = k.Idkorisnik,
                    Ime = k.Ime,
                    Prezime = k.Prezime,
                    Datumrodjenja = k.Datumrodjenja,
                    Email = k.Email,
                    Username = k.Username,
                    Pwdhash = k.Pwdhash,
                    Pwdsalt = k.Pwdsalt,
                    Telefon = k.Telefon,
                    Ulogaid = k.Ulogaid
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

        [HttpGet("[action]")]
        public ActionResult GenerirajToken()
        {
            try
            {
                var secureKey = _configuration["JWT:SecureKey"];
                var serializedToken = JwtTokenProvider.CreateToken(secureKey, 10);

                return Ok(serializedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
            //try
            //{
            //    var genericLoginFail = "Incorrect username or password";

            //    var existingUser = _context.Korisniks.FirstOrDefault(x => x.Username == korisnikLoginDTO.Username);
            //    if (existingUser == null)
            //        return BadRequest(genericLoginFail);

            //    var b64hash = PasswordHashProvider.GetHash(korisnikLoginDTO.Password, existingUser.Pwdsalt);
            //    if (b64hash != existingUser.Pwdhash)
            //        return BadRequest(genericLoginFail);

            //    var secureKey = _configuration["JWT:SecureKey"];
            //    var serializedToken = JwtTokenProvider.CreateToken(secureKey, 120, korisnikLoginDTO.Username);


            //    return Ok(serializedToken);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, ex.Message);
            //}
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
