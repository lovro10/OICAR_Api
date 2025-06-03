using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIUnitTests
{
    public class KOrisnikImageControllerTests
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
        public void CreateKorisnikImage_Success_AddsRecord()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikImageController(context);

            var dto = new KorisnikImageDTO
            {
                KorisnikId = 1,
                ImageId = 2
            };

            var result = controller.CreateKorisnikImage(dto);

            Assert.IsType<OkResult>(result.Result);

            var saved = context.Korisnikimages.SingleOrDefault(x =>
                x.Korisnikid == 1 && x.Imageid == 2);
            Assert.NotNull(saved);
        }
    }
}
