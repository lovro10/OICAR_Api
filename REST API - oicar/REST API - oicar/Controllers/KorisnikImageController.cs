using Microsoft.AspNetCore.Mvc;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikImageController : ControllerBase
    {
        private readonly CarshareContext _context;

        public KorisnikImageController(CarshareContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<KorisnikImageDTO> Get()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("[action]")]
        public ActionResult<KorisnikImageDTO> CreateKorisnikImage([FromBody] KorisnikImageDTO korisnikImageDto)
        {
            var korisnikImage = new Korisnikimage
            {
                Korisnikid = korisnikImageDto.KorisnikId,
                Imageid = korisnikImageDto.ImageId 
            }; 

            _context.Korisnikimages.Add(korisnikImage);
            _context.SaveChanges();

            return Ok();
        } 

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}