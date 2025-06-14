﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using REST_API___oicar.Security;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikController : ControllerBase
    {
        private readonly CarshareContext _context;
        private readonly IConfiguration _configuration;
        private readonly AesEncryptionService _encryptionService;

        public KorisnikController(CarshareContext context, IConfiguration configuration, AesEncryptionService encryptionService)
        {
            _context = context;
            _configuration = configuration;
            _encryptionService = encryptionService;
        } 

        [HttpPut("{id}")]
        public async Task<ActionResult<KorisnikUpdateDTO>> Update(int id, [FromBody] KorisnikUpdateDTO korisnikDto)
        {
            try
            {
                var korisnik = await _context.Korisniks.FindAsync(id);

                if (korisnik == null)
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");

                korisnik.Ime = _encryptionService.Encrypt(korisnikDto.Ime);
                korisnik.Prezime = _encryptionService.Encrypt(korisnikDto.Prezime);
                korisnik.Email = _encryptionService.Encrypt(korisnikDto.Email);
                korisnik.Telefon = _encryptionService.Encrypt(korisnikDto.Telefon);

                _context.Korisniks.Update(korisnik);
                await _context.SaveChangesAsync();

                return Ok(korisnikDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("Clear/{id}")]
        public async Task<ActionResult> ClearUserInfo(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks.FindAsync(id);

                if (korisnik == null)
                    return NotFound($"Korisnik with ID {id} not found");

                korisnik.Username = _encryptionService.Encrypt($"Anonymous_username_{id}");
                korisnik.Ime = _encryptionService.Encrypt($"Anonymous_name_{id}");
                korisnik.Prezime = _encryptionService.Encrypt($"Anonymous_surname_{id}");
                korisnik.Email = _encryptionService.Encrypt($"Anonymous_email_{id}");
                korisnik.Telefon = _encryptionService.Encrypt($"Anonymous_number_{id}"); 
                korisnik.Pwdhash = _encryptionService.Encrypt($"Anonymous_pwdhash_{id}");
                korisnik.Pwdsalt = _encryptionService.Encrypt($"Anonymous_pwdsalt_{id}");
                korisnik.Isconfirmed = null;
                korisnik.Ulogaid = 4;

                _context.Korisniks.Update(korisnik);
                await _context.SaveChangesAsync();

                return Ok($"All data for user with ID {id} are cleared");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("[action]")]
        public async Task<ActionResult> RequestClearInfo(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks.FindAsync(id);

                if (korisnik == null)
                    return NotFound($"Korisnik with ID {id} not found");

                korisnik.Ulogaid = 5;

                _context.Korisniks.Update(korisnik);
                await _context.SaveChangesAsync();

                return Ok($"User with {id} requested clearance");
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
                .Where(x => x.Ulogaid != 4 && x.Ulogaid != 5)
                .Include(x => x.Uloga)
                .Select(k => new KorisnikDTO
                {
                    IDKorisnik = k.Idkorisnik,
                    Ime = _encryptionService.Decrypt(k.Ime),
                    Prezime = _encryptionService.Decrypt(k.Prezime),
                    DatumRodjenja = k.Datumrodjenja,
                    Email = _encryptionService.Decrypt(k.Email),
                    Username = _encryptionService.Decrypt(k.Username),
                    Pwdhash = k.Pwdhash,
                    Pwdsalt = k.Pwdsalt,
                    Telefon = _encryptionService.Decrypt(k.Telefon),
                    UlogaId = k.Ulogaid,
                    Uloga = k.Uloga,
                    Isconfirmed = k.Isconfirmed,
                })
                .ToListAsync();
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<KorisnikDTO>>> GetAllRequestClear()
        { 
            return await _context.Korisniks 
                .Where(x => x.Ulogaid == 4 || x.Ulogaid == 5)
                .Include(x => x.Uloga)
                .Select(k => new KorisnikDTO
                {
                    IDKorisnik = k.Idkorisnik,
                    Ime = _encryptionService.Decrypt(k.Ime), 
                    Prezime = _encryptionService.Decrypt(k.Prezime),
                    DatumRodjenja = k.Datumrodjenja,
                    Email = _encryptionService.Decrypt(k.Email),
                    Username = _encryptionService.Decrypt(k.Username),
                    Pwdhash = k.Pwdhash,
                    Pwdsalt = k.Pwdsalt,
                    Telefon = _encryptionService.Decrypt(k.Telefon),
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

        [HttpGet("[action]")]
        public async Task<ActionResult<KorisnikDTO>> Details(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks
                    .Include(x => x.Korisnikimages).ThenInclude(x => x.Image)
                    .Include(x => x.Uloga)
                    .FirstOrDefaultAsync(x => x.Idkorisnik == id);

                if (korisnik == null)
                {
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");
                }

                var decryptedIme = _encryptionService.Decrypt(korisnik.Ime);
                var decryptedPrezime = _encryptionService.Decrypt(korisnik.Prezime);
                var decryptedEmail = _encryptionService.Decrypt(korisnik.Email);
                var decryptedTelefon = _encryptionService.Decrypt(korisnik.Telefon ?? "");
                var decryptedUsername = _encryptionService.Decrypt(korisnik.Username);
                var decryptedDate = _encryptionService.Decrypt(korisnik.Username);

                var imagesType1 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 1)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    }).ToList();

                var imagesType2 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 2)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    }).ToList();

                var imagesType3 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 3)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    }).ToList();

                var korisnikDTO = new KorisnikDTO
                {
                    IDKorisnik = korisnik.Idkorisnik,
                    Ime = decryptedIme,
                    Prezime = decryptedPrezime,
                    Email = decryptedEmail,
                    Username = decryptedUsername,
                    Telefon = decryptedTelefon,
                    DatumRodjenja = korisnik.Datumrodjenja,
                    Uloga = korisnik.Uloga,
                    Pwdhash = korisnik.Pwdhash,
                    Pwdsalt = korisnik.Pwdsalt,
                    UlogaId = korisnik.Ulogaid,
                    Isconfirmed = korisnik.Isconfirmed,
                    ImagesType1 = imagesType1,
                    ImagesType2 = imagesType2,
                    ImagesType3 = imagesType3
                };

                return Ok(korisnikDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<KorisnikDTO>> Profile(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks
                    .Include(x => x.Uloga)
                    .FirstOrDefaultAsync(x => x.Idkorisnik == id);

                if (korisnik == null)
                {
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");
                }

                var decryptedIme = _encryptionService.Decrypt(korisnik.Ime);
                var decryptedPrezime = _encryptionService.Decrypt(korisnik.Prezime);
                var decryptedEmail = _encryptionService.Decrypt(korisnik.Email);
                var decryptedTelefon = _encryptionService.Decrypt(korisnik.Telefon ?? "");
                var decryptedUsername = _encryptionService.Decrypt(korisnik.Username);
                
                var korisnikDTO = new KorisnikDTO
                {
                    IDKorisnik = korisnik.Idkorisnik,
                    Ime = decryptedIme,
                    Prezime = decryptedPrezime,
                    Email = decryptedEmail,
                    Username = decryptedUsername,
                    Telefon = decryptedTelefon,
                    DatumRodjenja = korisnik.Datumrodjenja,
                    Uloga = korisnik.Uloga
                };

                return Ok(korisnikDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<RegistracijaVozacDTO>> RegistracijaVozac([FromBody] string jsonRegistracijaVozacDTO)
        {
            try
            {
                var registracijaVozacDTO = JsonConvert.DeserializeObject<RegistracijaVozacDTO>(jsonRegistracijaVozacDTO);


                if (_context.Korisniks.Any(u => u.Username == jsonRegistracijaVozacDTO.Trim()))
                    return BadRequest("Username exists");


                var encryptedIme = _encryptionService.Encrypt(registracijaVozacDTO.Ime);
                var encryptedPrezime = _encryptionService.Encrypt(registracijaVozacDTO.Prezime);
                var encryptedEmail = _encryptionService.Encrypt(registracijaVozacDTO.Email);
                var encryptedTelefon = _encryptionService.Encrypt(registracijaVozacDTO.Telefon);
                var encryptedUsername = _encryptionService.Encrypt(registracijaVozacDTO.Username);
                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registracijaVozacDTO.Password, b64salt);

                var user = new Korisnik
                {
                    Username = encryptedUsername,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = encryptedIme,
                    Prezime = encryptedPrezime,
                    Email = encryptedEmail,
                    Telefon = encryptedTelefon,
                    Datumrodjenja = registracijaVozacDTO.Datumrodjenja,
                    Ulogaid = 2,
                    Isconfirmed = false
                };

                _context.Korisniks.Add(user);
                await _context.SaveChangesAsync();

                registracijaVozacDTO.Id = user.Idkorisnik;

                return Ok(registracijaVozacDTO.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<KorisnikRegistracijaDTO>> Registracija([FromBody] KorisnikRegistracijaDTO registracijaKorisnikDTO)
        {

            if (registracijaKorisnikDTO.Username == null)
                return BadRequest("Username is required");

            var encryptedUsername = _encryptionService.Encrypt(registracijaKorisnikDTO.Username.Trim());

            if (_context.Korisniks.Any(u => u.Username == encryptedUsername))
                return BadRequest("Username exists");

            var encryptedIme = _encryptionService.Encrypt(registracijaKorisnikDTO.Ime);
            var encryptedPrezime = _encryptionService.Encrypt(registracijaKorisnikDTO.Prezime);
            var encryptedEmail = _encryptionService.Encrypt(registracijaKorisnikDTO.Email);
            var encryptedTelefon = _encryptionService.Encrypt(registracijaKorisnikDTO.Telefon);

            var b64salt = PasswordHashProvider.GetSalt();
            var b64hash = PasswordHashProvider.GetHash(registracijaKorisnikDTO.Password, b64salt);

            var user = new Korisnik
            {
                Username = encryptedUsername,
                Pwdhash = b64hash,
                Pwdsalt = b64salt,
                Ime = encryptedIme,
                Prezime = encryptedPrezime,
                Email = encryptedEmail,
                Telefon = encryptedTelefon,
                Datumrodjenja = registracijaKorisnikDTO.Datumrodjenja,
                Ulogaid = 3
            };

            _context.Korisniks.Add(user);
            await _context.SaveChangesAsync();

            registracijaKorisnikDTO.Id = user.Idkorisnik;
            return Ok(registracijaKorisnikDTO);
        }

        [HttpGet("decrypted")]
        public async Task<ActionResult<object>> GetDecryptedUser([FromQuery] int userId)
        {
            var user = await _context.Korisniks.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            try
            {
                var decryptedIme = _encryptionService.Decrypt(user.Ime);
                var decryptedPrezime = _encryptionService.Decrypt(user.Prezime);
                var decryptedEmail = _encryptionService.Decrypt(user.Email);
                var decryptedTelefon = _encryptionService.Decrypt(user.Telefon);
                var decryptedUsername = _encryptionService.Decrypt(user.Username);

                var decryptedUser = new
                {
                    user.Idkorisnik,
                    Username = decryptedUsername,
                    Ime = decryptedIme,
                    Prezime = decryptedPrezime,
                    Email = decryptedEmail,
                    Telefon = decryptedTelefon,
                    user.Datumrodjenja,
                    user.Ulogaid
                };

                return Ok(decryptedUser);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Decryption failed: {ex.Message}");
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
                var encryptedUsername = _encryptionService.Encrypt(trimmedUsername);

                if (_context.Korisniks.Any(x => x.Username == encryptedUsername))
                    return BadRequest($"Username {trimmedUsername} already exists");
                var encryptedIme = _encryptionService.Encrypt(registracijaPutnikDTO.Ime);
                var encryptedPrezime = _encryptionService.Encrypt(registracijaPutnikDTO.Prezime);
                var encryptedEmail = _encryptionService.Encrypt(registracijaPutnikDTO.Email);
                var encryptedTelefon = _encryptionService.Encrypt(registracijaPutnikDTO.Telefon);

                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registracijaPutnikDTO.Password, b64salt);

                var user = new Korisnik
                {
                    Username = encryptedUsername,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = encryptedIme,
                    Prezime = encryptedPrezime,
                    Email = encryptedEmail,
                    Telefon = encryptedTelefon,
                    Datumrodjenja = registracijaPutnikDTO.Datumrodjenja,
                    Ulogaid = 1,
                    Isconfirmed = false
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

                var users = _context.Korisniks.Include(x => x.Uloga).ToList();

                var existingUser = users.FirstOrDefault(user =>
                {
                    try
                    {
                        var decrypted = _encryptionService.Decrypt(user.Username);
                        return decrypted == korisnikLoginDTO.Username;
                    }
                    catch
                    {

                        return false;
                    }
                });

                if (existingUser == null)
                    return Unauthorized(genericLoginFail);

                var hash = PasswordHashProvider.GetHash(korisnikLoginDTO.Password, existingUser.Pwdsalt);
                if (hash != existingUser.Pwdhash)
                    return Unauthorized(genericLoginFail);

                var secureKey = _configuration["Jwt:SecureKey"];
                var decryptedUsername = _encryptionService.Decrypt(existingUser.Username);

                var token = JwtTokenProvider.CreateToken(
                    secureKey,
                    120,
                    existingUser.Idkorisnik,
                    existingUser.Uloga.Naziv,
                    decryptedUsername
                );

                return Ok(JsonConvert.SerializeObject(token));
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
