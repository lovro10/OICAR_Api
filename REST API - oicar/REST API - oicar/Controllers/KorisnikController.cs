using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using REST_API___oicar.Models;
using REST_API___oicar.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Collections;

namespace REST_API___oicar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KorisnikController : ControllerBase
    {
        private readonly CarshareContext _context;
        private readonly IConfiguration _configuration;

        public KorisnikController(CarshareContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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


        [HttpPost("registracija")]
        public async Task<IActionResult> Registracija([FromBody] KorisnikRegistracijaDTO registracija)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Korisniks.AnyAsync(k => k.Username == registracija.Username || k.Email == registracija.Email))
                return BadRequest("Korisnik s tim korisničkim imenom ili emailom već postoji.");

            using var hmac = new HMACSHA512();
            byte[] salt = hmac.Key;
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registracija.Password));
            string hashString = Convert.ToBase64String(hash);
            string saltString = Convert.ToBase64String(salt);

            var noviKorisnik = new Korisnik
            {
                Ime = registracija.Ime,
                Prezime = registracija.Prezime,
                Datumrodjenja = registracija.Datumrodjenja,
                Email = registracija.Email,
                Username = registracija.Username,
                Pwdhash = hashString,
                Pwdsalt = saltString,
                Telefon = registracija.Telefon,
                Isconfirmed = new BitArray(new bool[] { false }),
                Ulogaid = registracija.Uloga
            };

            _context.Korisniks.Add(noviKorisnik);
            await _context.SaveChangesAsync();

            return Ok(new { poruka = "Registracija uspješna." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] KorisnikLoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var korisnik = await _context.Korisniks
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (korisnik == null)
                return Unauthorized("Neispravni podaci za prijavu.");

            byte[] salt = Convert.FromBase64String(korisnik.Pwdsalt);
            using var hmac = new HMACSHA512(salt);
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            string computedHashString = Convert.ToBase64String(computedHash);

            if (computedHashString != korisnik.Pwdhash)
                return Unauthorized("Neispravni podaci za prijavu.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentNullException(nameof(keyString), "JWT key not found in configuration.");

            var key = Encoding.UTF8.GetBytes(keyString);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, korisnik.Idkorisnik.ToString()),
                    new Claim(ClaimTypes.Name, korisnik.Username),
                    new Claim("Uloga", korisnik.Ulogaid?.ToString() ?? "1")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
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
;            existingKorisnik.Telefon = korisnikDto.Telefon;
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


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var korisnik = await _context.Korisniks.FindAsync(id);
            if (korisnik == null)
                return NotFound();

            _context.Korisniks.Remove(korisnik);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("promjenalozinke")]
        public async Task<IActionResult> PromjenaLozinke([FromBody] KorisnikPromjenaLozinkeDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var korisnik = await _context.Korisniks.FirstOrDefaultAsync(k => k.Email == dto.Email);
            if (korisnik == null)
                return NotFound("Korisnik ne postoji.");

            byte[] salt = Convert.FromBase64String(korisnik.Pwdsalt);
            using var hmac = new HMACSHA512(salt);
            byte[] computedOldHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.OldPassword));
            string computedOldHashString = Convert.ToBase64String(computedOldHash);

            if (computedOldHashString != korisnik.Pwdhash)
                return Unauthorized("Stara lozinka je netočna.");

            using var hmacNew = new HMACSHA512();
            byte[] newSalt = hmacNew.Key;
            byte[] newHash = hmacNew.ComputeHash(Encoding.UTF8.GetBytes(dto.NewPassword));
            string newHashString = Convert.ToBase64String(newHash);
            string newSaltString = Convert.ToBase64String(newSalt);

            korisnik.Pwdhash = newHashString;
            korisnik.Pwdsalt = newSaltString;
            _context.Korisniks.Update(korisnik);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Dogodila se greška prilikom promjene lozinke: {ex.Message}");
            }

            return Ok(new { poruka = "Lozinka uspješno promijenjena." });
        }


    }
}
