using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Newtonsoft.Json;
using System.Text.Json;
using REST_API___oicar.Security;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoziloController : ControllerBase
    {
        private readonly CarshareContext _context;

        private readonly AesEncryptionService _encryptionService;

        public VoziloController(CarshareContext context, AesEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Vozilo>>> GetVehicles()
        {
            var vozila = await _context.Vozilos
                .Include(v => v.Imageprometna)
                .OrderByDescending(o => o.Idvozilo)
                .Select(v => new
                {
                    v.Idvozilo,
                    v.Naziv,
                    v.Marka,
                    v.Model,
                    v.Registracija,
                    v.Isconfirmed,
                    v.Vozac.Idkorisnik,
                    Ime = _encryptionService.Decrypt(v.Vozac.Ime),
                    Prezime = _encryptionService.Decrypt(v.Vozac.Prezime),
                    Username = _encryptionService.Decrypt(v.Vozac.Username)
                })
                .ToListAsync();

            return Ok(vozila);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Vozilo>> GetVehicleById(int id)
        {
            var vozilo = await _context.Vozilos
                .Where(v => v.Idvozilo == id)
                .Select(v => new
                {
                    v.Idvozilo,
                    v.Naziv,
                    v.Marka,
                    v.Model,
                    v.Registracija,
                    v.Isconfirmed,
                    v.Vozac.Idkorisnik,
                    Ime = _encryptionService.Decrypt(v.Vozac.Ime),
                    Prezime = _encryptionService.Decrypt(v.Vozac.Prezime),
                    Username = _encryptionService.Decrypt(v.Vozac.Username)
                })
                .FirstOrDefaultAsync();

            if (vozilo == null)
                return NotFound();

            return Ok(vozilo);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<VoziloDTO>> Details(int id)
        {
            try
            {
                var vozilo = await _context.Vozilos
                    .Include(x => x.Vozac)
                        .ThenInclude(x => x.Korisnikimages)
                            .ThenInclude(x => x.Image)
                    .FirstOrDefaultAsync(x => x.Idvozilo == id);

                if (vozilo == null)
                {
                    return NotFound($"Vozilo sa ID-jem {id} nije pronađeno.");
                }

                var vozacImages = vozilo.Vozac?.Korisnikimages
                    .Where(x => x.Image != null && x.Image.Imagetypeid == 4) 
                    .Select(x => new ImageDTO
                    { 
                        Idimage = x.Image.Idimage,
                        Name = x.Image.Name,
                        ContentBase64 = Convert.ToBase64String(x.Image.Content),
                        ImageTypeId = x.Image.Imagetypeid,
                        ImageTypeName = x.Image.Imagetype?.Name
                    })
                    .ToList() ?? new List<ImageDTO>();

                var voziloDTO = new VoziloDTO
                {
                    Idvozilo = vozilo.Idvozilo,
                    Marka = vozilo.Marka,
                    Model = vozilo.Model,
                    Registracija = vozilo.Registracija,
                    Isconfirmed = vozilo.Isconfirmed,

                    Vozac = new VozacDTO
                    {
                        Idkorisnik = vozilo.Vozac?.Idkorisnik ?? 0,
                        Ime = _encryptionService.Decrypt(vozilo.Vozac?.Ime),
                        Prezime = _encryptionService.Decrypt(vozilo.Vozac?.Prezime),
                        Username = _encryptionService.Decrypt(vozilo.Vozac?.Username) 
                    }
                };

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<VoziloDetaljiDTO>> DetailsVehicle(int id)
        {
            try
            {
                var vozilo = await _context.Vozilos
                    .Include(x => x.Vozac)
                    .FirstOrDefaultAsync(v => v.Idvozilo == id);

                if (vozilo == null)
                    return NotFound($"Vozilo sa ID-jem {id} nije pronađeno.");

                var imageDTOs = new List<ImageDTO>();
                if (vozilo.Imageprometnaid.HasValue)
                {
                    int frontId = vozilo.Imageprometnaid.Value;
                    int backId = frontId + 1;

                    var images = await _context.Images
                        .Include(x => x.Imagetype)
                        .Where(img => (img.Idimage == frontId || img.Idimage == backId) && img.Imagetypeid == 4)
                        .ToListAsync();

                    imageDTOs = images.Select(img => new ImageDTO
                    {
                        Idimage = img.Idimage,
                        Name = img.Name,
                        ContentBase64 = Convert.ToBase64String(img.Content),
                        ImageTypeId = img.Imagetypeid,
                        ImageTypeName = img.Imagetype?.Name
                    }).ToList();
                }

                var voziloDTO = new VoziloDTO
                {
                    Idvozilo = vozilo.Idvozilo,
                    Naziv = vozilo.Naziv,  
                    Marka = vozilo.Marka,
                    Model = vozilo.Model,
                    Registracija = vozilo.Registracija,
                    Isconfirmed = vozilo.Isconfirmed,

                    Vozac = new VozacDTO
                    {
                        Idkorisnik = vozilo.Vozac?.Idkorisnik ?? 0,
                        Ime = _encryptionService.Decrypt(vozilo.Vozac?.Ime),
                        Prezime = _encryptionService.Decrypt(vozilo.Vozac?.Prezime),
                        Username = _encryptionService.Decrypt(vozilo.Vozac?.Username) 
                    }
                };

                var result = new VoziloDetaljiDTO 
                {
                    Vozilo = voziloDTO,
                    IdentificationImages = imageDTOs
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška na serveru: {ex.Message}");
            }
        }

        [HttpGet("[action]")]
        public ActionResult<IEnumerable<VoziloDTO>> GetVehicleByUser([FromQuery] int userId)
        {
            var vozila = _context.Vozilos
                .Where(v => v.Vozacid == userId)
                .OrderByDescending(o => o.Idvozilo)
                .Select(v => new VoziloDTO
                {
                    Idvozilo = v.Idvozilo,
                    Naziv = v.Naziv, 
                    Marka = v.Marka,
                    Model = v.Model,
                    Registracija = v.Registracija,
                    VozacId = v.Vozacid, 
                    Isconfirmed = v.Isconfirmed 
                })
                .ToList();

            Console.WriteLine($"Retrieved {vozila.Count} vehicles for User ID: {userId}.");
            foreach (var vozilo in vozila)
            {
                Console.WriteLine($"Vehicle ID: {vozilo.Idvozilo}, Marka: {vozilo.Marka}, Model: {vozilo.Model}");
            }

            return Ok(vozila);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<VoziloDTO>> KrerirajVozilo([FromBody] VoziloDTO voziloDTO)
        {
            try
            {
                var vozilo = new Vozilo
                {
                    Naziv = voziloDTO.Naziv,
                    Marka = voziloDTO.Marka,
                    Model = voziloDTO.Model,
                    Registracija = voziloDTO.Registracija,
                    Vozacid = voziloDTO.VozacId, 
                    Isconfirmed = false 
                };

                _context.Vozilos.Add(vozilo);
                await _context.SaveChangesAsync();

                voziloDTO.Idvozilo = vozilo.Idvozilo;

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        } 

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateVehicle([FromBody] VoziloDTO voziloDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(voziloDTO.FrontImageBase64) || string.IsNullOrEmpty(voziloDTO.BackImageBase64))
                {
                    return BadRequest("Both front and back images are required.");
                }

                var frontImageContent = Convert.FromBase64String(voziloDTO.FrontImageBase64);

                var backImageContent = Convert.FromBase64String(voziloDTO.BackImageBase64);

                var frontImage = new Image
                {
                    Name = voziloDTO.FrontImageName,
                    Content = frontImageContent,  
                    Imagetypeid = 4  
                }; 

                var backImage = new Image
                { 
                    Name = voziloDTO.BackImageName,
                    Content = backImageContent, 
                    Imagetypeid = 4  
                };

                _context.Images.Add(frontImage);
                _context.Images.Add(backImage);
                await _context.SaveChangesAsync();

                var vozilo = new Vozilo
                {
                    Naziv = voziloDTO.Naziv,
                    Marka = voziloDTO.Marka,
                    Model = voziloDTO.Model,
                    Registracija = voziloDTO.Registracija,
                    Vozacid = voziloDTO.VozacId,
                    Isconfirmed = false, 
                    Imageprometnaid = frontImage.Idimage 
                };

                _context.Vozilos.Add(vozilo);
                await _context.SaveChangesAsync();
                
                var korisnikImageFront = new Korisnikimage
                {
                    Korisnikid = voziloDTO.VozacId.Value, 
                    Imageid = frontImage.Idimage 
                };

                var korisnikImageBack = new Korisnikimage
                {
                    Korisnikid = voziloDTO.VozacId.Value,  
                    Imageid = backImage.Idimage 
                }; 

                _context.Korisnikimages.Add(korisnikImageFront);
                _context.Korisnikimages.Add(korisnikImageBack);
                await _context.SaveChangesAsync();

                var createdVoziloDTO = new VoziloDTO
                {
                    Idvozilo = vozilo.Idvozilo,
                    Naziv = vozilo.Naziv,
                    Marka = vozilo.Marka,
                    Model = vozilo.Model,
                    Registracija = vozilo.Registracija,
                    VozacId = vozilo.Vozacid.Value,
                    FrontImageBase64 = voziloDTO.FrontImageBase64, 
                    BackImageBase64 = voziloDTO.BackImageBase64, 
                    FrontImageName = frontImage.Name, 
                    BackImageName = backImage.Name 
                }; 

                return Ok(createdVoziloDTO); 
            } 
            catch (Exception ex)
            { 
                return StatusCode(500, ex.Message); 
            }
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult<VoziloDTO>> UpdateVehicle(int id, [FromBody] String jsonVoziloDTO)
        { 
            var voziloDTO = JsonConvert.DeserializeObject<VoziloDTO>(jsonVoziloDTO);

            try
            {
                var vozilo = await _context.Vozilos.FindAsync(id);

                if (vozilo == null)
                {
                    return NotFound($"Vozilo sa ID-jem {id} nije pronađeno.");
                }

                vozilo.Marka = voziloDTO.Marka;
                vozilo.Model = voziloDTO.Model;
                vozilo.Registracija = voziloDTO.Registracija;

                _context.Vozilos.Update(vozilo);
                await _context.SaveChangesAsync();

                voziloDTO.Idvozilo = vozilo.Idvozilo;

                return Ok(voziloDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vozilo = await _context.Vozilos.FindAsync(id);
            if (vozilo == null)
                return NotFound();

            _context.Vozilos.Remove(vozilo);
            await _context.SaveChangesAsync();

            return Ok(vozilo);
        }

        [HttpPut("[action]")] 
        public async Task<IActionResult> AcceptOrDenyVehicle([FromBody] PotvrdaVoziloDTO potvrdaVoziloDTO)   
        { 
            var vozilo = await _context.Vozilos.FindAsync(potvrdaVoziloDTO.Id); 

            if (vozilo == null)
                return NotFound("Vehicle was not found");

            vozilo.Isconfirmed = potvrdaVoziloDTO.IsConfirmed; 

            _context.Entry(vozilo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var status = (bool)vozilo.Isconfirmed ? "confirmed" : "denied"; 
            return Ok($"Vehicle was successfully {status}"); 
        }
    }
}

