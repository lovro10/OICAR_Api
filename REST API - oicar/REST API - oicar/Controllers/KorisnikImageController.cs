using Microsoft.AspNetCore.Mvc;
using REST_API___oicar.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        // GET: api/<KorisnikImage>
        [HttpGet]
        public ActionResult<KorisnikImageDTO> Get()
        {
            return Ok();
        }

        // GET api/<KorisnikImage>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<KorisnikImage>
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

        // PUT api/<KorisnikImage>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<KorisnikImage>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}