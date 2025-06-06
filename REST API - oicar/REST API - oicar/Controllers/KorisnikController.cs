using Microsoft.AspNetCore.Mvc;
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

        [HttpPut("Clear/{id}")]
        public async Task<ActionResult> ClearUserInfo(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks.FindAsync(id);

                if (korisnik == null)
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");

                korisnik.Username = $"deleted_username_{id}";
                korisnik.Ime = ""; 
                korisnik.Prezime = "";
                korisnik.Email = $"deleted_email_{id}";
                korisnik.Telefon = "";
                korisnik.Datumrodjenja = default;
                korisnik.Pwdhash = "";
                korisnik.Pwdsalt = "";
                korisnik.Isconfirmed = null;
                korisnik.Ulogaid = 4;

                _context.Korisniks.Update(korisnik);
                await _context.SaveChangesAsync();

                return Ok($"Svi podaci korisnika sa ID-jem {id} su obrisani.");
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

        [HttpGet("[action]")]
        public async Task<ActionResult<KorisnikDTO>> Details(int id)
        {
            try
            {
                var korisnik = await _context.Korisniks
                    .Include(x => x.Korisnikimages)
                        .ThenInclude(x => x.Image)
                    .Include(x => x.Uloga) 
                    .FirstOrDefaultAsync(x => x.Idkorisnik == id);

                if (korisnik == null)
                {
                    return NotFound($"Korisnik sa ID-jem {id} nije pronađen.");
                }

                var imagesType1 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 1)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    })
                    .ToList();

                var imagesType2 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 2)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    })
                    .ToList();

                var imagesType3 = korisnik.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 3)
                    .Select(x => new ImageDTO
                    {
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    })
                    .ToList();

                var korisnikDTO = new KorisnikDTO
                {
                    IDKorisnik = korisnik.Idkorisnik,
                    Ime = korisnik.Ime,
                    Prezime = korisnik.Prezime,
                    Email = korisnik.Email,
                    Username = korisnik.Username,
                    Telefon = korisnik.Telefon ?? "",
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

        [HttpPost("[action]")]
        public async Task<ActionResult<RegistracijaVozacDTO>> RegistracijaVozac([FromBody] String jsonRegistracijaVozacDTO)
        {
            try
            {
                var registracijaVozacDTO = JsonConvert.DeserializeObject<RegistracijaVozacDTO>(jsonRegistracijaVozacDTO);

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
            try
            {
                var trimmedUsername = registracijaKorisnikDTO.Username.Trim();
                if (_context.Korisniks.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                var b64salt = PasswordHashProvider.GetSalt(); 
                var b64hash = PasswordHashProvider.GetHash(registracijaKorisnikDTO.Password, b64salt);

                var user = new Korisnik 
                { 
                    Username = registracijaKorisnikDTO.Username,
                    Pwdhash = b64hash,
                    Pwdsalt = b64salt,
                    Ime = registracijaKorisnikDTO.Ime,
                    Prezime = registracijaKorisnikDTO.Prezime,
                    Email = registracijaKorisnikDTO.Email,
                    Telefon = registracijaKorisnikDTO.Telefon,
                    Datumrodjenja = registracijaKorisnikDTO.Datumrodjenja,
                    Ulogaid = 3 
                }; 
                
                _context.Korisniks.Add(user);
                await _context.SaveChangesAsync();

                registracijaKorisnikDTO.Id = user.Idkorisnik;

                return Ok(registracijaKorisnikDTO);
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

                var existingUser = _context.Korisniks
                    .Include(x => x.Uloga)
                    .FirstOrDefault(x => x.Username == korisnikLoginDTO.Username);
                if (existingUser == null)
                {
                    return Unauthorized(genericLoginFail);
                }

                var b64hash = PasswordHashProvider.GetHash(korisnikLoginDTO.Password, existingUser.Pwdsalt);
                if (b64hash != existingUser.Pwdhash)
                {
                    return Unauthorized(genericLoginFail);
                }

                var secureKey = _configuration["Jwt:SecureKey"];

                var token = JwtTokenProvider.CreateToken(
                    secureKey,
                    120,
                    existingUser.Idkorisnik,
                    existingUser.Uloga.Naziv, 
                    existingUser.Username 
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
