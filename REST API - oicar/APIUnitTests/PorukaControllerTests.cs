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
    public class PorukaControllerTests
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
        public async Task GetMessagesForRide_ReturnsOrderedMessagesWithSenderName()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "putnikUser",
                Ime = "Putnik",
                Prezime = "P",
                Email = "p@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "vozacUser",
                Ime = "Vozac",
                Prezime = "V",
                Email = "v@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };

            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 2,
                Marka = "Mk",
                Model = "Md",
                Registracija = "REG",
                Vozac = vozac
            };
            var rideAd = new Oglasvoznja
            {
                Idoglasvoznja = 20,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 3,
                Troskoviid = 0,
                Lokacijaid = 0,
                Statusvoznjeid = 0
            };
            var korisnikVoznja = new Korisnikvoznja
            {
                Idkorisnikvoznja = 30,
                Korisnikid = 1,
                Oglasvoznjaid = 20,
                Korisnik = putnik,
                Oglasvoznja = rideAd,
                Lokacijaputnik = "Start",
                Lokacijavozac = "Start"
            };

            var message1 = new Poruka
            {
                Idporuka = 100,
                Content = "Hello from putnik",
                Korisnikvoznjaid = 30,
                Putnikid = 1,
                Vozacid = null
            };
            var message2 = new Poruka
            {
                Idporuka = 101,
                Content = "Reply from vozac",
                Korisnikvoznjaid = 30,
                Putnikid = null,
                Vozacid = 2
            };

            context.Korisniks.AddRange(putnik, vozac);
            context.Vozilos.Add(vehicle);
            context.Oglasvoznjas.Add(rideAd);
            context.Korisnikvoznjas.Add(korisnikVoznja);
            context.Porukas.AddRange(message1, message2);
            await context.SaveChangesAsync();

            var controller = new PorukaController(context);
            var actionResult = await controller.GetMessagesForRide(30);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<PorukaGetDTO>>(ok.Value);
            var messages = list.ToList();

            Assert.Equal(2, messages.Count);
            Assert.Equal(100, messages[0].Idporuka);
            Assert.Equal("Hello from putnik", messages[0].Content);
            Assert.Equal(30, messages[0].KorisnikVoznjaId);
            Assert.Equal(1, messages[0].PutnikId);
            Assert.Null(messages[0].VozacId);
            Assert.Equal("putnikUser", messages[0].SenderName);

            Assert.Equal(101, messages[1].Idporuka);
            Assert.Equal("Reply from vozac", messages[1].Content);
            Assert.Equal(30, messages[1].KorisnikVoznjaId);
            Assert.Null(messages[1].PutnikId);
            Assert.Equal(2, messages[1].VozacId);
            Assert.Equal("vozacUser", messages[1].SenderName);
        }

        [Fact]
        public async Task SendMessageForRide_MissingIdOrContent_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new PorukaController(context);

            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 0,
                PutnikId = 1,
                Content = ""
            };
            var result = await controller.SendMessageForRide(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("KorisnikVoznjaId i sadržaj", bad.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_NoSenderProvided_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new PorukaController(context);

            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                PutnikId = null,
                VozacId = null,
                Content = "Test"
            };
            var result = await controller.SendMessageForRide(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Pošiljatelj mora biti definiran", bad.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_BothSenderIdsProvided_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new PorukaController(context);

            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                PutnikId = 1,
                VozacId = 2,
                Content = "Test"
            };
            var result = await controller.SendMessageForRide(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Poruku može poslati samo jedan korisnik", bad.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_KorisnikVoznjaNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new PorukaController(context);

            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 999, 
                PutnikId = 1,
                Content = "Hi"
            };
            var result = await controller.SendMessageForRide(dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Voznja ne postoji", notFound.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_PutnikIdMismatch_ReturnsForbid()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "putnikUser",
                Ime = "Putnik",
                Prezime = "P",
                Email = "p@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "vozacUser",
                Ime = "Vozac",
                Prezime = "V",
                Email = "v@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 2,
                Marka = "Mk",
                Model = "Md",
                Registracija = "REG",
                Vozac = vozac
            };
            var rideAd = new Oglasvoznja
            {
                Idoglasvoznja = 20,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 3,
                Troskoviid = 0,
                Lokacijaid = 0,
                Statusvoznjeid = 0
            };
            var korisnikVoznja = new Korisnikvoznja
            {
                Idkorisnikvoznja = 30,
                Korisnikid = 1, 
                Oglasvoznjaid = 20,
                Korisnik = putnik,
                Oglasvoznja = rideAd,
                Lokacijaputnik = "Start",
                Lokacijavozac = "Start"
            };

            context.Korisniks.AddRange(putnik, vozac);
            context.Vozilos.Add(vehicle);
            context.Oglasvoznjas.Add(rideAd);
            context.Korisnikvoznjas.Add(korisnikVoznja);
            await context.SaveChangesAsync();

            var controller = new PorukaController(context);
            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                PutnikId = 999,
                Content = "Hello"
            };
            var result = await controller.SendMessageForRide(dto);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SendMessageForRide_VozacIdMismatch_ReturnsForbid()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "putnikUser",
                Ime = "Putnik",
                Prezime = "P",
                Email = "p@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "vozacUser",
                Ime = "Vozac",
                Prezime = "V",
                Email = "v@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 2, 
                Marka = "Mk",
                Model = "Md",
                Registracija = "REG",
                Vozac = vozac
            };
            var rideAd = new Oglasvoznja
            {
                Idoglasvoznja = 20,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 3,
                Troskoviid = 0,
                Lokacijaid = 0,
                Statusvoznjeid = 0
            };
            var korisnikVoznja = new Korisnikvoznja
            {
                Idkorisnikvoznja = 30,
                Korisnikid = 1,
                Oglasvoznjaid = 20,
                Korisnik = putnik,
                Oglasvoznja = rideAd,
                Lokacijaputnik = "Start",
                Lokacijavozac = "Start"
            };

            context.Korisniks.AddRange(putnik, vozac);
            context.Vozilos.Add(vehicle);
            context.Oglasvoznjas.Add(rideAd);
            context.Korisnikvoznjas.Add(korisnikVoznja);
            await context.SaveChangesAsync();

            var controller = new PorukaController(context);
            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                VozacId = 999,
                Content = "Hello"
            };
            var result = await controller.SendMessageForRide(dto);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SendMessageForRide_SuccessAsPutnik_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "putnikUser",
                Ime = "Putnik",
                Prezime = "P",
                Email = "p@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "vozacUser",
                Ime = "Vozac",
                Prezime = "V",
                Email = "v@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 2,
                Marka = "Mk",
                Model = "Md",
                Registracija = "REG",
                Vozac = vozac
            };
            var rideAd = new Oglasvoznja
            {
                Idoglasvoznja = 20,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 3,
                Troskoviid = 0,
                Lokacijaid = 0,
                Statusvoznjeid = 0
            };
            var korisnikVoznja = new Korisnikvoznja
            {
                Idkorisnikvoznja = 30,
                Korisnikid = 1,
                Oglasvoznjaid = 20,
                Korisnik = putnik,
                Oglasvoznja = rideAd,
                Lokacijaputnik = "Start",
                Lokacijavozac = "Start"
            };

            context.Korisniks.AddRange(putnik, vozac);
            context.Vozilos.Add(vehicle);
            context.Oglasvoznjas.Add(rideAd);
            context.Korisnikvoznjas.Add(korisnikVoznja);
            await context.SaveChangesAsync();

            var controller = new PorukaController(context);
            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                PutnikId = 1,
                Content = "Hello from putnik"
            };
            var result = await controller.SendMessageForRide(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Message sent", ok.Value.ToString());

            var saved = await context.Porukas.FirstOrDefaultAsync(x =>
                x.Korisnikvoznjaid == 30 && x.Putnikid == 1 && x.Content == "Hello from putnik");
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task SendMessageForRide_SuccessAsVozac_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "putnikUser",
                Ime = "Putnik",
                Prezime = "P",
                Email = "p@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "vozacUser",
                Ime = "Vozac",
                Prezime = "V",
                Email = "v@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 2,
                Marka = "Mk",
                Model = "Md",
                Registracija = "REG",
                Vozac = vozac
            };
            var rideAd = new Oglasvoznja
            {
                Idoglasvoznja = 20,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 3,
                Troskoviid = 0,
                Lokacijaid = 0,
                Statusvoznjeid = 0
            };
            var korisnikVoznja = new Korisnikvoznja
            {
                Idkorisnikvoznja = 30,
                Korisnikid = 1,
                Oglasvoznjaid = 20,
                Korisnik = putnik,
                Oglasvoznja = rideAd,
                Lokacijaputnik = "Start",
                Lokacijavozac = "Start"
            };

            context.Korisniks.AddRange(putnik, vozac);
            context.Vozilos.Add(vehicle);
            context.Oglasvoznjas.Add(rideAd);
            context.Korisnikvoznjas.Add(korisnikVoznja);
            await context.SaveChangesAsync();

            var controller = new PorukaController(context);
            var dto = new PorukaSendDTO
            {
                KorisnikVoznjaId = 30,
                VozacId = 2,
                Content = "Hello from vozac"
            };
            var result = await controller.SendMessageForRide(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Message sent", ok.Value.ToString());

            var saved = await context.Porukas.FirstOrDefaultAsync(x =>
                x.Korisnikvoznjaid == 30 && x.Vozacid == 2 && x.Content == "Hello from vozac");
            Assert.NotNull(saved);
        }
    }
}
