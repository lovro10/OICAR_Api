using Microsoft.AspNetCore.Mvc;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitySearchController : ControllerBase
    {
        private List<string> cities = new List<string>();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAllCroatianCities()
        {
            string filePath = Path.Combine("Files", "cities.txt");

            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        cities.Add(line.Trim());
                    }
                }
            }
            else
            {
                return NotFound("File not found.");
            }

            return Ok(cities);
        }
    }
}