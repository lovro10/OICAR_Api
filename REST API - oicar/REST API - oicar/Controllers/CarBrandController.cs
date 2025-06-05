using Microsoft.AspNetCore.Mvc;

namespace REST_API___oicar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarBrandController : Controller
    {
        private List<string> cars = new List<string>();

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<string>>> GetAllCarBrands() 
        { 
            string filePath = Path.Combine("Files", "Cars.txt");

            if (System.IO.File.Exists(filePath))
            { 
                string[] lines = System.IO.File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        cars.Add(line.Trim());
                    } 
                } 
            } 
            else
            { 
                return NotFound("File not found.");
            } 

            return Ok(cars);
        }
    }
} 