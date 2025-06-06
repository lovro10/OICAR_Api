using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{
    public class OglasVoziloControllerTests
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
        public async Task GetReservedDates_ReturnsAllDatesBetweenReservations()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var reservation1 = new Korisnikvozilo
            {
                Idkorisnikvozilo = 1,
                Korisnikid = 10,
                Oglasvoziloid = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 01, 08, 0, 0),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 03, 18, 0, 0)
            };
            var reservation2 = new Korisnikvozilo
            {
                Idkorisnikvozilo = 2,
                Korisnikid = 11,
                Oglasvoziloid = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 05, 09, 0, 0),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 06, 17, 0, 0)
            };

            context.Korisnikvozilos.AddRange(reservation1, reservation2);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.GetReservedDates(1);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dates = Assert.IsAssignableFrom<List<string>>(ok.Value);

            var expected = new HashSet<string>
            {
                "2023-01-01",
                "2023-01-02",
                "2023-01-03",
                "2023-01-05",
                "2023-01-06"
            };
            Assert.Equal(expected, dates.ToHashSet());
        }

        [Fact]
        public async Task CreateReservation_NoOverlap_ReturnsOkWithReservation()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var existing = new Korisnikvozilo
            {
                Idkorisnikvozilo = 1,
                Korisnikid = 10,
                Oglasvoziloid = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 01, 12, 0, 0),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 03, 12, 0, 0)
            };
            context.Korisnikvozilos.Add(existing);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var model = new VehicleReservationDTO
            {
                KorisnikId = 20,
                OglasVoziloId = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 04),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 05)
            };

            var actionResult = await controller.CreateReservation(model);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<Korisnikvozilo>(ok.Value);

            Assert.Equal(20, returned.Korisnikid);
            Assert.Equal(1, returned.Oglasvoziloid);
            Assert.Equal(new DateTime(2023, 01, 04, 12, 0, 0), returned.DatumPocetkaRezervacije);
            Assert.Equal(new DateTime(2023, 01, 05, 12, 0, 0), returned.DatumZavrsetkaRezervacije);

            // Verify it was saved
            var saved = await context.Korisnikvozilos
                .FirstOrDefaultAsync(x => x.Korisnikid == 20 && x.Oglasvoziloid == 1);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task CreateReservation_Overlap_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var existing = new Korisnikvozilo
            {
                Idkorisnikvozilo = 1,
                Korisnikid = 10,
                Oglasvoziloid = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 10, 12, 0, 0),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 12, 12, 0, 0)
            };
            context.Korisnikvozilos.Add(existing);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var overlappingModel = new VehicleReservationDTO
            {
                KorisnikId = 20,
                OglasVoziloId = 1,
                DatumPocetkaRezervacije = new DateTime(2023, 01, 11),
                DatumZavrsetkaRezervacije = new DateTime(2023, 01, 13)
            };

            var actionResult = await controller.CreateReservation(overlappingModel);
            var bad = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Contains("overlap", bad.Value.ToString());
        }

        [Fact]
        public async Task GetAllByUser_ReturnsOnlyUserAds()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 5,
                Username = "driver5",
                Ime = "D",
                Prezime = "R",
                Email = "d@r",
                Telefon = "000",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle1 = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 5,
                Marka = "MkA",
                Model = "MdA",
                Registracija = "REGA",
                Vozac = driver
            };
            var vehicle2 = new Vozilo
            {
                Idvozilo = 11,
                Vozacid = 6, 
                Marka = "MkB",
                Model = "MdB",
                Registracija = "REGB"
            };
            var ad1 = new Oglasvozilo
            {
                Idoglasvozilo = 100,
                Voziloid = 10,
                Vozilo = vehicle1,
                DatumPocetkaRezervacije = new DateTime(2023, 02, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 02, 05)
            };
            var ad2 = new Oglasvozilo
            {
                Idoglasvozilo = 101,
                Voziloid = 11,
                Vozilo = vehicle2,
                DatumPocetkaRezervacije = new DateTime(2023, 03, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 03, 05)
            };
            context.Korisniks.Add(driver);
            context.Vozilos.AddRange(vehicle1, vehicle2);
            context.Oglasvozilos.AddRange(ad1, ad2);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.GetAllByUser(5);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<OglasVoziloDTO>>(ok.Value);

            Assert.Single(list);
            var returned = list.First();
            Assert.Equal(100, returned.IdOglasVozilo);
            Assert.Equal(10, returned.VoziloId);
            Assert.Equal("MkA", returned.Marka);
            Assert.Equal("MdA", returned.Model);
            Assert.Equal("REGA", returned.Registracija);
            Assert.Equal(5, returned.KorisnikId);
            Assert.Equal("driver5", returned.Username);
        }

        [Fact]
        public async Task GetAll_ReturnsAllAdsOrderedDescending()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 5,
                Username = "drv5",
                Ime = "D",
                Prezime = "R",
                Email = "d@r",
                Telefon = "000",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 20,
                Vozacid = 5,
                Marka = "MkX",
                Model = "MdX",
                Registracija = "REGX",
                Vozac = driver
            };
            var adOld = new Oglasvozilo
            {
                Idoglasvozilo = 200,
                Voziloid = 20,
                Vozilo = vehicle,
                DatumPocetkaRezervacije = new DateTime(2023, 04, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 04, 05)
            };
            var adNew = new Oglasvozilo
            {
                Idoglasvozilo = 201,
                Voziloid = 20,
                Vozilo = vehicle,
                DatumPocetkaRezervacije = new DateTime(2023, 05, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 05, 05)
            };
            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Oglasvozilos.AddRange(adOld, adNew);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<OglasVoziloDTO>>(ok.Value);

            var ordered = list.ToList();
            Assert.Equal(2, ordered.Count);
            Assert.Equal(201, ordered[0].IdOglasVozilo);
            Assert.Equal(200, ordered[1].IdOglasVozilo);
        }

        [Fact]
        public async Task GetOglasVoziloById_Existing_ReturnsDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 7,
                Username = "drv7",
                Ime = "D7",
                Prezime = "R7",
                Email = "d7@r",
                Telefon = "777",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 30,
                Vozacid = 7,
                Marka = "MkZ",
                Model = "MdZ",
                Registracija = "REGZ",
                Vozac = driver
            };
            var ad = new Oglasvozilo
            {
                Idoglasvozilo = 300,
                Voziloid = 30,
                Vozilo = vehicle,
                DatumPocetkaRezervacije = new DateTime(2023, 06, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 06, 05)
            };
            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Oglasvozilos.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.GetOglasVoziloById(300);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<OglasVoziloDTO>(ok.Value);

            Assert.Equal(300, dto.IdOglasVozilo);
            Assert.Equal(30, dto.VoziloId);
            Assert.Equal("MkZ", dto.Marka);
            Assert.Equal("MdZ", dto.Model);
            Assert.Equal("REGZ", dto.Registracija);
            Assert.Equal(7, dto.KorisnikId);
            Assert.Equal("drv7", dto.Username);
        }

        [Fact]
        public async Task GetOglasVoziloById_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);

            var actionResult = await controller.GetOglasVoziloById(999);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task DetaljiOglasaVozila_Existing_ReturnsDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 9,
                Username = "drv9",
                Ime = "D9",
                Prezime = "R9",
                Email = "d9@r",
                Telefon = "999",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 40,
                Vozacid = 9,
                Marka = "MkY",
                Model = "MdY",
                Registracija = "REGY",
                Vozac = driver
            };
            var ad = new Oglasvozilo
            {
                Idoglasvozilo = 400,
                Voziloid = 40,
                Vozilo = vehicle,
                DatumPocetkaRezervacije = new DateTime(2023, 07, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 07, 05)
            };
            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Oglasvozilos.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.DetaljiOglasaVozila(400);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<OglasVoziloDTO>(ok.Value);

            Assert.Equal(400, dto.IdOglasVozilo);
            Assert.Equal("MkY", dto.Marka);
            Assert.Equal("MdY", dto.Model);
            Assert.Equal("REGY", dto.Registracija);
            Assert.Equal("drv9", dto.Username);
        }

        [Fact]
        public async Task DetaljiOglasaVozila_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);

            var actionResult = await controller.DetaljiOglasaVozila(999);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task KreirajOglasVozilo_InvalidModel_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);
            controller.ModelState.AddModelError("error", "invalid");

            var dto = new OglasVoziloDTO();
            var actionResult = await controller.KreirajOglasVozilo(dto);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task KreirajOglasVozilo_DuplicateVehicle_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var adExisting = new Oglasvozilo
            {
                Idoglasvozilo = 500,
                Voziloid = 50,
                DatumPocetkaRezervacije = new DateTime(2023, 08, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 08, 05)
            };
            context.Oglasvozilos.Add(adExisting);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var dto = new OglasVoziloDTO
            {
                VoziloId = 50,
                DatumPocetkaRezervacije = new DateTime(2023, 08, 10),
                DatumZavrsetkaRezervacije = new DateTime(2023, 08, 15)
            };

            var actionResult = await controller.KreirajOglasVozilo(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Contains("already exists", bad.Value.ToString());
        }

        [Fact]
        public async Task KreirajOglasVozilo_Success_ReturnsDtoWithId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var controller = new OglasVoziloController(context);
            var dto = new OglasVoziloDTO
            {
                VoziloId = 60,
                DatumPocetkaRezervacije = new DateTime(2023, 09, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 09, 05)
            };

            var actionResult = await controller.KreirajOglasVozilo(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<OglasVoziloDTO>(ok.Value);

            Assert.True(returned.IdOglasVozilo > 0);
            Assert.Equal(60, returned.VoziloId);

            var saved = await context.Oglasvozilos.FindAsync(returned.IdOglasVozilo);
            Assert.NotNull(saved);
            Assert.Equal(60, saved.Voziloid);
        }

        [Fact]
        public async Task AzurirajOglasVozilo_InvalidModel_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);
            controller.ModelState.AddModelError("error", "invalid");

            var dto = new OglasVoziloDTO();
            var actionResult = await controller.AzurirajOglasVozilo(1, dto);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task AzurirajOglasVozilo_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);

            var dto = new OglasVoziloDTO
            {
                VoziloId = 70,
                DatumPocetkaRezervacije = new DateTime(2023, 10, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 10, 05)
            };
            var actionResult = await controller.AzurirajOglasVozilo(999, dto);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task AzurirajOglasVozilo_Success_ReturnsDtoWithUpdatedValues()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var ad = new Oglasvozilo
            {
                Idoglasvozilo = 800,
                Voziloid = 80,
                DatumPocetkaRezervacije = new DateTime(2023, 11, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 11, 05)
            };
            context.Oglasvozilos.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var dto = new OglasVoziloDTO
            {
                VoziloId = 80,
                DatumPocetkaRezervacije = new DateTime(2023, 11, 10),
                DatumZavrsetkaRezervacije = new DateTime(2023, 11, 15)
            };

            var actionResult = await controller.AzurirajOglasVozilo(800, dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<OglasVoziloDTO>(ok.Value);

            Assert.Equal(800, returned.IdOglasVozilo);
            Assert.Equal(new DateTime(2023, 11, 10), returned.DatumPocetkaRezervacije);
            Assert.Equal(new DateTime(2023, 11, 15), returned.DatumZavrsetkaRezervacije);

            var saved = await context.Oglasvozilos.FindAsync(800);
            Assert.Equal(new DateTime(2023, 11, 10), saved.DatumPocetkaRezervacije);
            Assert.Equal(new DateTime(2023, 11, 15), saved.DatumZavrsetkaRezervacije);
        }

        [Fact]
        public async Task ObrisiOglasVozilo_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoziloController(context);

            var actionResult = await controller.ObrisiOglasVozilo(999);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task ObrisiOglasVozilo_Success_ReturnsDeletedEntity()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var vehicle = new Vozilo
            {
                Idvozilo = 90,
                Vozacid = 9,
                Marka = "MkDel",
                Model = "MdDel",
                Registracija = "REGDEL"
            };
            var ad = new Oglasvozilo
            {
                Idoglasvozilo = 900,
                Voziloid = 90,
                Vozilo = vehicle,
                DatumPocetkaRezervacije = new DateTime(2023, 12, 01),
                DatumZavrsetkaRezervacije = new DateTime(2023, 12, 05)
            };
            context.Vozilos.Add(vehicle);
            context.Oglasvozilos.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoziloController(context);
            var actionResult = await controller.ObrisiOglasVozilo(900);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<Oglasvozilo>(ok.Value);

            Assert.Equal(900, returned.Idoglasvozilo);
            Assert.Null(await context.Oglasvozilos.FindAsync(900));
        }
    }
}
