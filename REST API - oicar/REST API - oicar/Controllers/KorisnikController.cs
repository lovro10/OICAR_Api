using Microsoft.AspNetCore.Mvc;
using REST_API___oicar.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikController : ControllerBase
    {
        private readonly CarshareContext _context;

        public KorisnikController(CarshareContext context)
        {
            _context = context;
        }

        // GET: api/<KorisnikController>
        [HttpGet]
        public ActionResult<IEnumerable<Korisnik>> Get()
        {
            //this was just a test! Replace with your code (Bartol)
            return _context.Korisniks.Select(x => new Korisnik
            {
                Idkorisnik = x.Idkorisnik,
                Ime = x.Ime,
                Prezime = x.Prezime,
                Datumrodjenja = x.Datumrodjenja,
                Email = x.Email,
                Username = x.Username,
                Pwdhash = x.Pwdhash,
                Pwdsalt = x.Pwdsalt,
                Telefon = x.Telefon

            }).ToList();
        }

        // GET api/<KorisnikController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<KorisnikController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<KorisnikController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<KorisnikController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
