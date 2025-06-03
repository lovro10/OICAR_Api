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
    public class KorisnikVoznjaControllerTests
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
        public async Task GetAll_ReturnsAllJoinedRides()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                BrojPutnika = 3,
                Lokacija = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" },
                DatumIVrijemePolaska = DateTime.Now.AddHours(1)
            };
            var prijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 5,
                Korisnikid = 1,
                Oglasvoznjaid = 2,
                Lokacijaputnik = "Lp",
                Lokacijavozac = "Lv",
                Korisnik = user,
                Oglasvoznja = oglas
            };
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            context.Korisnikvoznjas.Add(prijava);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var result = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<KorisnikVoznjaDTO>>(ok.Value);

            Assert.Single(list);
            Assert.Equal(5, list.First().IdKorisnikVoznja);
            Assert.Equal(1, list.First().KorisnikId);
            Assert.Equal(2, list.First().OglasVoznjaId);
        }

        [Fact]
        public async Task JoinRide_Success_ReturnsDtoWithId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                BrojPutnika = 1,
                Lokacija = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" },
                DatumIVrijemePolaska = DateTime.Now.AddHours(2),
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var dto = new KorisnikVoznjaDTO
            {
                KorisnikId = 1,
                OglasVoznjaId = 2,
                LokacijaPutnik = "Lp",
                LokacijaVozac = "Lv"
            };
            var result = await controller.JoinRide(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<KorisnikVoznjaDTO>(ok.Value);

            Assert.True(returned.IdKorisnikVoznja > 0);

            var saved = context.Korisnikvoznjas.SingleOrDefault(x =>
                x.Korisnikid == 1 && x.Oglasvoznjaid == 2);
            Assert.NotNull(saved);
            Assert.Equal(returned.IdKorisnikVoznja, saved.Idkorisnikvoznja);
        }

        [Fact]
        public async Task JoinRide_ModelInvalid_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikVoznjaController(context);
            controller.ModelState.AddModelError("error", "invalid");

            var dto = new KorisnikVoznjaDTO();
            var result = await controller.JoinRide(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task JoinRide_RideNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikVoznjaController(context);

            var dto = new KorisnikVoznjaDTO
            {
                KorisnikId = 1,
                OglasVoznjaId = 99
            };
            var result = await controller.JoinRide(dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Contains("Vožnja nije pronađena", notFound.Value.ToString());
        }

        [Fact]
        public async Task JoinRide_AlreadyJoined_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                BrojPutnika = 2,
                Lokacija = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" },
                DatumIVrijemePolaska = DateTime.Now.AddHours(2),
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            var prijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 5,
                Korisnikid = 1,
                Oglasvoznjaid = 2,
                Lokacijaputnik = "Lp",
                Lokacijavozac = "Lv"
            };
            oglas.Korisnikvoznjas.Add(prijava);
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            context.Korisnikvoznjas.Add(prijava);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var dto = new KorisnikVoznjaDTO
            {
                KorisnikId = 1,
                OglasVoznjaId = 2,
                LokacijaPutnik = "Lp",
                LokacijaVozac = "Lv"
            };
            var result = await controller.JoinRide(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("već prijavljen", bad.Value.ToString());
        }

        [Fact]
        public async Task JoinRide_NoSeats_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                BrojPutnika = 1,
                Lokacija = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" },
                DatumIVrijemePolaska = DateTime.Now.AddHours(2),
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            var existing = new Korisnikvoznja
            {
                Idkorisnikvoznja = 5,
                Korisnikid = 2,
                Oglasvoznjaid = 2,
                Lokacijaputnik = "X",
                Lokacijavozac = "Y"
            };
            oglas.Korisnikvoznjas.Add(existing);
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            context.Korisnikvoznjas.Add(existing);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var dto = new KorisnikVoznjaDTO
            {
                KorisnikId = 1,
                OglasVoznjaId = 2,
                LokacijaPutnik = "Lp",
                LokacijaVozac = "Lv"
            };
            var result = await controller.JoinRide(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("Nema slobodnih mjesta", bad.Value.ToString());
        }

        [Fact]
        public async Task UserJoinedRide_InvalidInput_ReturnsBadRequestFalse()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikVoznjaController(context);

            var actionResult = await controller.UserJoinedRide(0, 0);
            var bad = Assert.IsType<BadRequestObjectResult>(actionResult);

            Assert.False((bool)bad.Value);
        }

        [Fact]
        public async Task UserJoinedRide_AsPassenger_ReturnsTrue()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                Vozilo = new Vozilo
                {
                    Idvozilo = 3,
                    Vozacid = 5,
                    Marka = "DummyMarka",
                    Model = "DummyModel",
                    Registracija = "XYZ123"
                },
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            var prijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 6,
                Korisnikid = 1,
                Oglasvoznjaid = 2,
                Lokacijaputnik = "Lp",
                Lokacijavozac = "Lv"
            };
            oglas.Korisnikvoznjas.Add(prijava);
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            context.Korisnikvoznjas.Add(prijava);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var actionResult = await controller.UserJoinedRide(1, 2);
            var ok = Assert.IsType<OkObjectResult>(actionResult);

            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task UserJoinedRide_AsDriver_ReturnsTrue()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var driver = new Korisnik
            {
                Idkorisnik = 5,
                Username = "drv",
                Ime = "D",
                Prezime = "R",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                Vozilo = new Vozilo
                {
                    Idvozilo = 3,
                    Vozacid = 5,
                    Marka = "DummyMarka",
                    Model = "DummyModel",
                    Registracija = "XYZ123"
                },
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            context.Korisniks.Add(driver);
            context.Oglasvoznjas.Add(oglas);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var actionResult = await controller.UserJoinedRide(5, 2);
            var ok = Assert.IsType<OkObjectResult>(actionResult);

            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task UserJoinedRide_NotJoinedOrDriver_ReturnsFalse()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                Vozilo = new Vozilo
                {
                    Idvozilo = 3,
                    Vozacid = 5,
                    Marka = "DummyMarka",
                    Model = "DummyModel",
                    Registracija = "XYZ123"
                },
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var actionResult = await controller.UserJoinedRide(1, 2);
            var ok = Assert.IsType<OkObjectResult>(actionResult);

            Assert.False((bool)ok.Value);
        }

        [Fact]
        public async Task LeaveRide_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var oglas = new Oglasvoznja { Idoglasvoznja = 2 };
            var prijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 5,
                Korisnikid = 1,
                Oglasvoznjaid = 2
            };
            context.Korisniks.Add(user);
            context.Oglasvoznjas.Add(oglas);
            context.Korisnikvoznjas.Add(prijava);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var dto = new KorisnikVoznjaDTO { KorisnikId = 1, OglasVoznjaId = 2 };
            var actionResult = await controller.LeaveRide(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);

            Assert.Contains("Successfully left the ride", ok.Value.ToString());

            var remaining = context.Korisnikvoznjas.SingleOrDefault(x =>
                x.Korisnikid == 1 && x.Oglasvoznjaid == 2);
            Assert.Null(remaining);
        }

        [Fact]
        public async Task LeaveRide_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikVoznjaController(context);

            var dto = new KorisnikVoznjaDTO { KorisnikId = 99, OglasVoznjaId = 100 };
            var actionResult = await controller.LeaveRide(dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult);

            Assert.Contains("User did not enter", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetHistoryOfRides_ReturnsPastRidesOnly()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "t",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var pastOglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                DatumIVrijemePolaska = DateTime.Now.AddDays(-1),
                Lokacija = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" }
            };
            var futureOglas = new Oglasvoznja
            {
                Idoglasvoznja = 3,
                DatumIVrijemePolaska = DateTime.Now.AddDays(1),
                Lokacija = new Lokacija { Idlokacija = 2, Polaziste = "C", Odrediste = "D" }
            };
            var pastPrijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 5,
                Korisnikid = 1,
                Oglasvoznjaid = 2,
                Oglasvoznja = pastOglas,
                Lokacijaputnik = "Lp",
                Lokacijavozac = "Lv"
            };
            var futurePrijava = new Korisnikvoznja
            {
                Idkorisnikvoznja = 6,
                Korisnikid = 1,
                Oglasvoznjaid = 3,
                Oglasvoznja = futureOglas,
                Lokacijaputnik = "Xp",
                Lokacijavozac = "Xv"
            };
            context.Korisniks.Add(user);
            context.Oglasvoznjas.AddRange(pastOglas, futureOglas);
            context.Korisnikvoznjas.AddRange(pastPrijava, futurePrijava);
            await context.SaveChangesAsync();

            var controller = new KorisnikVoznjaController(context);
            var actionResult = await controller.GetHistoryOfRides(1);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var history = Assert.IsAssignableFrom<IEnumerable<VoznjaHistoryDTO>>(ok.Value);
            var list = history.ToList();

            Assert.Single(list);
            Assert.Equal(2, list[0].Oglasvoznjaid);
            Assert.Equal(pastOglas.DatumIVrijemePolaska, list[0].DatumVoznje);
        }
    }
}
