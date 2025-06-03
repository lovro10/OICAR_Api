using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using REST_API___oicar.Controllers;
/*
 * provjeravamo rad metode GetAllCroatianCities u kontroleru koji cita popis gradova iz datoteke
 */
namespace APIUnitTests
{
    public class CitySearchControllerTests
    {
        private string _originalCwd;
        /*
         * stvaramo novi folder da ne diramo stvarne datoteke
         * pamtimo pocetni folder u kojemu je app pokrenuta
         * radni folder prebacimo na novi privremeni tako da kontroler gleda taj privremeni folder
         * nakon svakog testa vracamo se u pocetni folder i brisemo taj privremeni
         */
        private void SetUpTemporaryDirectory(out string tempDir)
        {
            _originalCwd = Directory.GetCurrentDirectory();

            tempDir = Path.Combine(_originalCwd, "TempTestDir", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            Directory.SetCurrentDirectory(tempDir);
        }

        private void TearDownTemporaryDirectory(string tempDir)
        {
            Directory.SetCurrentDirectory(_originalCwd);

            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch
                {
                    //TODO
                }
            }
        }
        /*
         * Test da vidimo da li kontroler ispravno cita gradove kad datoteka postoji
         */
        [Fact]
        public async Task GetAllCroatianCities_FileExists_ReturnsOkWithTrimmedList()
        {
            SetUpTemporaryDirectory(out var tempDir);
            try
            {
                Directory.CreateDirectory("Files");
                var lines = new[]
                {
                    "  Zagreb  ",
                    "",            
                    " Split ",
                    " Rijeka",     
                    "Dubrovnik  "  
                };
                File.WriteAllLines(Path.Combine("Files", "cities.txt"), lines);

                var controller = new CitySearchController();
                var result = await controller.GetAllCroatianCities();

                var ok = Assert.IsType<OkObjectResult>(result.Result);
                var returned = Assert.IsAssignableFrom<List<string>>(ok.Value);

                var expected = new List<string>
                {
                    "Zagreb",
                    "Split",
                    "Rijeka",
                    "Dubrovnik"
                };
                Assert.Equal(expected, returned);
            }
            finally
            {
                TearDownTemporaryDirectory(tempDir);
            }
        }
        /*
         * Test da vidimo da li kontroler ispravno cita gradove kad datoteka ne postoji
         * vraca 404 not found ili poruku no found
         */
        [Fact]
        public async Task GetAllCroatianCities_FileMissing_ReturnsNotFound()
        {
            SetUpTemporaryDirectory(out var tempDir);
            try
            {

               
                var controller = new CitySearchController();
                var result = await controller.GetAllCroatianCities();

                var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
                Assert.Equal("File not found.", notFound.Value);
            }
            finally
            {
                TearDownTemporaryDirectory(tempDir);
            }
        }
    }
}
