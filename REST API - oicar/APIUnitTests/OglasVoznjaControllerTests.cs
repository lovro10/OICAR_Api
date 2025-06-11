using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Models;  
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace APIUnitTests
{
    public class OglasVoznjeControllerTests
    {

        private CarshareContext CreateInMemoryContext(string dbName)
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<CarshareContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            return new CarshareContext(options);
        }

        [Fact]
        public async Task KreirajOglasVoznje_ReturnsOkResult_WhenModelIsValid()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);

            var dto = new OglasVoznjaDTO
            {
                Cestarina = 10,
                Gorivo = 50,
                Polaziste = "Sarajevo",
                Odrediste = "Mostar",
                VoziloId = 1,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(2),
                BrojPutnika = 1,
                KorisnikId = 1
            };

            var result = await controller.KreirajOglasVoznje(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<OglasVoznjaDTO>(okResult.Value);

            Assert.NotEqual(0, returnedDto.IdOglasVoznja);
            Assert.NotEqual(0, returnedDto.TroskoviId);
            Assert.NotEqual(0, returnedDto.LokacijaId);
        }

        [Fact]
        public async Task KreirajOglasVoznje_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);
            controller.ModelState.AddModelError("Polaziste", "Required");

            var dto = new OglasVoznjaDTO();

            var result = await controller.KreirajOglasVoznje(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}