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
    public class OglasVoznjaControllerTests
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
        public async Task GetAll_ReturnsListOfAllAds()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 1,
                Username = "driver1",
                Ime = "D",
                Prezime = "One",
                Email = "d1@o",
                Telefon = "123",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 1,
                Marka = "Mk",
                Model = "Md",
                Registracija = "ABC123",
                Vozac = driver
            };
            var troskovi = new Troskovi
            {
                Idtroskovi = 100,
                Cestarina = 50,
                Gorivo = 150
            };
            var lokacija = new Lokacija
            {
                Idlokacija = 200,
                Polaziste = "Start",
                Odrediste = "End"
            };
            var status = new Statusvoznje
            {
                Idstatusvoznje = 300,
                Naziv = "Active"
            };
            var ad1 = new Oglasvoznja
            {
                Idoglasvoznja = 1000,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = new DateTime(2023, 01, 01, 8, 0, 0),
                DatumIVrijemeDolaska = new DateTime(2023, 01, 01, 12, 0, 0),
                BrojPutnika = 4,
                Troskoviid = 100,
                Troskovi = troskovi,
                Lokacijaid = 200,
                Lokacija = lokacija,
                Statusvoznjeid = 300,
                Statusvoznje = status
            };
            var ad2 = new Oglasvoznja
            {
                Idoglasvoznja = 1001,
                Voziloid = 10,
                Vozilo = vehicle,
                DatumIVrijemePolaska = new DateTime(2023, 02, 01, 8, 0, 0),
                DatumIVrijemeDolaska = new DateTime(2023, 02, 01, 12, 0, 0),
                BrojPutnika = 2,
                Troskoviid = 100,
                Troskovi = troskovi,
                Lokacijaid = 200,
                Lokacija = lokacija,
                Statusvoznjeid = 300,
                Statusvoznje = status
            };

            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.AddRange(ad1, ad2);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var actionResult = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<OglasVoznjaDTO>>(ok.Value);
            var dtoList = list.ToList();

            Assert.Equal(2, dtoList.Count);
            Assert.Equal(1001, dtoList[0].IdOglasVoznja);
            Assert.Equal("Mk", dtoList[0].Marka);
            Assert.Equal("Md", dtoList[0].Model);
            Assert.Equal("ABC123", dtoList[0].Registracija);
            Assert.Equal("driver1", dtoList[0].Username);
            Assert.Equal(50, dtoList[0].Cestarina);
            Assert.Equal(150, dtoList[0].Gorivo);
            Assert.Equal("Start", dtoList[0].Polaziste);
            Assert.Equal("End", dtoList[0].Odrediste);
            Assert.Equal((50 + 150) / 2, dtoList[0].CijenaPoPutniku);

            Assert.Equal(1000, dtoList[1].IdOglasVoznja);
            Assert.Equal((50 + 150) / 4, dtoList[1].CijenaPoPutniku);
        }

        [Fact]
        public async Task GetAllByUser_ReturnsOnlyUserAds()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver1 = new Korisnik
            {
                Idkorisnik = 1,
                Username = "drv1",
                Ime = "D1",
                Prezime = "One",
                Email = "d1@o",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var driver2 = new Korisnik
            {
                Idkorisnik = 2,
                Username = "drv2",
                Ime = "D2",
                Prezime = "Two",
                Email = "d2@o",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1990, 2, 2),
                Pwdsalt = "",
                Pwdhash = ""
            };

            var vehicle1 = new Vozilo
            {
                Idvozilo = 10,
                Vozacid = 1,
                Marka = "M1",
                Model = "Mdl1",
                Registracija = "REG1",
                Vozac = driver1
            };
            var vehicle2 = new Vozilo
            {
                Idvozilo = 11,
                Vozacid = 2,
                Marka = "M2",
                Model = "Mdl2",
                Registracija = "REG2",
                Vozac = driver2
            };

            var troskovi = new Troskovi { Idtroskovi = 100, Cestarina = 20, Gorivo = 80 };
            var lokacija = new Lokacija { Idlokacija = 200, Polaziste = "S", Odrediste = "E" };
            var status = new Statusvoznje { Idstatusvoznje = 300, Naziv = "Active" };

            var adFor1 = new Oglasvoznja
            {
                Idoglasvoznja = 1000,
                Voziloid = 10,
                Vozilo = vehicle1,
                DatumIVrijemePolaska = new DateTime(2023, 5, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 5, 1).AddHours(4),
                BrojPutnika = 3,
                Troskoviid = 100,
                Troskovi = troskovi,
                Lokacijaid = 200,
                Lokacija = lokacija,
                Statusvoznjeid = 300,
                Statusvoznje = status
            };
            var adFor2 = new Oglasvoznja
            {
                Idoglasvoznja = 1001,
                Voziloid = 11,
                Vozilo = vehicle2,
                DatumIVrijemePolaska = new DateTime(2023, 6, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 6, 1).AddHours(4),
                BrojPutnika = 2,
                Troskoviid = 100,
                Troskovi = troskovi,
                Lokacijaid = 200,
                Lokacija = lokacija,
                Statusvoznjeid = 300,
                Statusvoznje = status
            };

            context.Korisniks.AddRange(driver1, driver2);
            context.Vozilos.AddRange(vehicle1, vehicle2);
            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.AddRange(adFor1, adFor2);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var actionResult = await controller.GetAllByUser(1);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<OglasVoznjaDTO>>(ok.Value);
            var dtoList = list.ToList();

            Assert.Single(dtoList);
            var dto = dtoList[0];
            Assert.Equal(1000, dto.IdOglasVoznja);
            Assert.Equal("M1", dto.Marka);
            Assert.Equal("Mdl1", dto.Model);
            Assert.Equal("REG1", dto.Registracija);
            Assert.Equal("drv1", dto.Username);
        }

        [Fact]
        public async Task GetById_Existing_ReturnsDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 5,
                Username = "drv5",
                Ime = "D5",
                Prezime = "Five",
                Email = "d5@o",
                Telefon = "555",
                Datumrodjenja = new DateOnly(1990, 5, 5),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 15,
                Vozacid = 5,
                Marka = "Mk5",
                Model = "Md5",
                Registracija = "REG5",
                Vozac = driver
            };
            var troskovi = new Troskovi { Idtroskovi = 105, Cestarina = 30, Gorivo = 70 };
            var lokacija = new Lokacija { Idlokacija = 205, Polaziste = "X", Odrediste = "Y" };
            var status = new Statusvoznje { Idstatusvoznje = 305, Naziv = "Pending" };
            var ad = new Oglasvoznja
            {
                Idoglasvoznja = 1005,
                Voziloid = 15,
                Vozilo = vehicle,
                DatumIVrijemePolaska = new DateTime(2023, 7, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 7, 1).AddHours(3),
                BrojPutnika = 2,
                Troskoviid = 105,
                Troskovi = troskovi,
                Lokacijaid = 205,
                Lokacija = lokacija,
                Statusvoznjeid = 305,
                Statusvoznje = status
            };

            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var actionResult = await controller.GetById(1005);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<OglasVoznjaDTO>(ok.Value);

            Assert.Equal(1005, dto.IdOglasVoznja);
            Assert.Equal("Mk5", dto.Marka);
            Assert.Equal("Md5", dto.Model);
            Assert.Equal("REG5", dto.Registracija);
            Assert.Equal(new DateTime(2023, 7, 1), dto.DatumIVrijemePolaska);
            Assert.Equal(2, dto.BrojPutnika);
            Assert.Equal(30, dto.Cestarina);
            Assert.Equal(70, dto.Gorivo);
            Assert.Equal("X", dto.Polaziste);
            Assert.Equal("Y", dto.Odrediste);
            Assert.Equal("Pending", dto.StatusVoznjeNaziv);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);

            var actionResult = await controller.GetById(9999);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task DetaljiOglasaVoznje_Existing_ReturnsDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 6,
                Username = "drv6",
                Ime = "D6",
                Prezime = "Six",
                Email = "d6@o",
                Telefon = "666",
                Datumrodjenja = new DateOnly(1990, 6, 6),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 16,
                Vozacid = 6,
                Marka = "Mk6",
                Model = "Md6",
                Registracija = "REG6",
                Vozac = driver
            };
            var troskovi = new Troskovi { Idtroskovi = 106, Cestarina = 40, Gorivo = 60 };
            var lokacija = new Lokacija { Idlokacija = 206, Polaziste = "A1", Odrediste = "B1" };
            var status = new Statusvoznje { Idstatusvoznje = 306, Naziv = "Confirmed" };
            var ad = new Oglasvoznja
            {
                Idoglasvoznja = 1006,
                Voziloid = 16,
                Vozilo = vehicle,
                DatumIVrijemePolaska = new DateTime(2023, 8, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 8, 1).AddHours(2),
                BrojPutnika = 3,
                Troskoviid = 106,
                Troskovi = troskovi,
                Lokacijaid = 206,
                Lokacija = lokacija,
                Statusvoznjeid = 306,
                Statusvoznje = status
            };

            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var actionResult = await controller.DetaljiOglasaVoznje(1006);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<OglasVoznjaDTO>(ok.Value);

            Assert.Equal(1006, dto.IdOglasVoznja);
            Assert.Equal("Mk6", dto.Marka);
            Assert.Equal("Md6", dto.Model);
            Assert.Equal("REG6", dto.Registracija);
            Assert.Equal("A1", dto.Polaziste);
            Assert.Equal("B1", dto.Odrediste);
            Assert.Equal("Confirmed", dto.StatusVoznjeNaziv);
        }

        [Fact]
        public async Task DetaljiOglasaVoznje_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);

            var actionResult = await controller.DetaljiOglasaVoznje(8888);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task KreirajOglasVoznje_InvalidModel_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);
            controller.ModelState.AddModelError("error", "invalid");

            var dto = new OglasVoznjaDTO();
            var actionResult = await controller.KreirajOglasVoznje(dto);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task KreirajOglasVoznje_Success_ReturnsDtoAndCreatesEntities()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 7,
                Username = "drv7",
                Ime = "D7",
                Prezime = "Seven",
                Email = "d7@o",
                Telefon = "777",
                Datumrodjenja = new DateOnly(1990, 7, 7),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle = new Vozilo
            {
                Idvozilo = 17,
                Vozacid = 7,
                Marka = "Mk7",
                Model = "Md7",
                Registracija = "REG7",
                Vozac = driver
            };
            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var dto = new OglasVoznjaDTO
            {
                VoziloId = 17,
                KorisnikId = 7,
                Cestarina = 100,
                Gorivo = 200,
                Polaziste = "P7",
                Odrediste = "O7",
                DatumIVrijemePolaska = new DateTime(2023, 9, 1, 9, 0, 0),
                DatumIVrijemeDolaska = new DateTime(2023, 9, 1, 13, 0, 0),
                BrojPutnika = 5
            };

            var actionResult = await controller.KreirajOglasVoznje(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<OglasVoznjaDTO>(ok.Value);

            Assert.True(returned.IdOglasVoznja > 0);
            Assert.True(returned.TroskoviId > 0);
            Assert.True(returned.LokacijaId > 0);
            Assert.True(returned.StatusVoznjeId > 0);

            var savedT = await context.Troskovis.FindAsync(returned.TroskoviId);
            Assert.NotNull(savedT);
            Assert.Equal(100, savedT.Cestarina);
            Assert.Equal(200, savedT.Gorivo);

            var savedL = await context.Lokacijas.FindAsync(returned.LokacijaId);
            Assert.NotNull(savedL);
            Assert.Equal("P7", savedL.Polaziste);
            Assert.Equal("O7", savedL.Odrediste);

            var savedS = await context.Statusvoznjes.FindAsync(returned.StatusVoznjeId);
            Assert.NotNull(savedS);
            Assert.Equal("Uskoro", savedS.Naziv);

            var savedAd = await context.Oglasvoznjas.FindAsync(returned.IdOglasVoznja);
            Assert.NotNull(savedAd);
            Assert.Equal(17, savedAd.Voziloid);
            Assert.Equal(5, savedAd.BrojPutnika);

            var savedKV = await context.Korisnikvoznjas
                .FirstOrDefaultAsync(kv => kv.Oglasvoznjaid == returned.IdOglasVoznja && kv.Korisnikid == 7);
            Assert.NotNull(savedKV);
            Assert.Equal("P7", savedKV.Lokacijavozac);
            Assert.Equal("P7", savedKV.Lokacijaputnik);
        }

        [Fact]
        public async Task AzurirajOglasVoznje_InvalidModel_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);
            controller.ModelState.AddModelError("error", "invalid");

            var dto = new OglasVoznjaDTO();
            var actionResult = await controller.AzurirajOglasVoznje(1, dto);
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }

        [Fact]
        public async Task AzurirajOglasVoznje_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);

            var dto = new OglasVoznjaDTO
            {
                Cestarina = 10,
                Gorivo = 20,
                Polaziste = "X",
                Odrediste = "Y",
                DatumIVrijemePolaska = DateTime.Now,
                DatumIVrijemeDolaska = DateTime.Now.AddHours(1),
                BrojPutnika = 1,
                VoziloId = 99
            };
            var actionResult = await controller.AzurirajOglasVoznje(9999, dto);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task AzurirajOglasVoznje_Success_UpdatesEntities()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var troskovi = new Troskovi { Idtroskovi = 110, Cestarina = 50, Gorivo = 50 };
            var lokacija = new Lokacija { Idlokacija = 210, Polaziste = "OldStart", Odrediste = "OldEnd" };
            var status = new Statusvoznje { Idstatusvoznje = 310, Naziv = "OldStatus" };
            var ad = new Oglasvoznja
            {
                Idoglasvoznja = 1010,
                Voziloid = 20,
                Troskoviid = 110,
                Troskovi = troskovi,
                Lokacijaid = 210,
                Lokacija = lokacija,
                Statusvoznjeid = 310,
                Statusvoznje = status,
                DatumIVrijemePolaska = new DateTime(2023, 10, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 10, 1).AddHours(2),
                BrojPutnika = 2
            };

            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var dto = new OglasVoznjaDTO
            {
                Cestarina = 60,              
                Gorivo = 40,                  
                Polaziste = "NewStart",      
                Odrediste = "NewEnd",         
                DatumIVrijemePolaska = new DateTime(2023, 11, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 11, 1).AddHours(3),
                BrojPutnika = 3,
                VoziloId = 21                  
            };

            var actionResult = await controller.AzurirajOglasVoznje(1010, dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<OglasVoznjaDTO>(ok.Value);

            Assert.Equal(1010, returned.IdOglasVoznja);
            Assert.Equal(60, returned.Cestarina.Value);
            Assert.Equal(40, returned.Gorivo.Value);
            Assert.Equal("NewStart", returned.Polaziste);
            Assert.Equal("NewEnd", returned.Odrediste);
            Assert.Equal(3, returned.BrojPutnika);
            Assert.Equal(21, returned.VoziloId);

            var updatedT = await context.Troskovis.FindAsync(returned.TroskoviId.Value);
            Assert.Equal(60, updatedT.Cestarina);
            Assert.Equal(40, updatedT.Gorivo);

            var updatedL = await context.Lokacijas.FindAsync(returned.LokacijaId.Value);
            Assert.Equal("NewStart", updatedL.Polaziste);
            Assert.Equal("NewEnd", updatedL.Odrediste);

            var updatedStatus = await context.Statusvoznjes.FindAsync(returned.StatusVoznjeId.Value);
            Assert.Equal("Ažurirano", updatedStatus.Naziv);

            var updatedAd = await context.Oglasvoznjas.FindAsync(1010);
            Assert.Equal(new DateTime(2023, 11, 1), updatedAd.DatumIVrijemePolaska);
            Assert.Equal(3, updatedAd.BrojPutnika);
            Assert.Equal(21, updatedAd.Voziloid);
        }

        [Fact]
        public async Task ObrisiOglasVoznje_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new OglasVoznjaController(context);

            var actionResult = await controller.ObrisiOglasVoznje(9999);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task ObrisiOglasVoznje_Success_DeletesEntities()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var troskovi = new Troskovi { Idtroskovi = 120, Cestarina = 10, Gorivo = 20 };
            var lokacija = new Lokacija { Idlokacija = 220, Polaziste = "DelStart", Odrediste = "DelEnd" };
            var status = new Statusvoznje { Idstatusvoznje = 320, Naziv = "ToDelete" };
            var ad = new Oglasvoznja
            {
                Idoglasvoznja = 1020,
                Voziloid = 30,
                Troskoviid = 120,
                Troskovi = troskovi,
                Lokacijaid = 220,
                Lokacija = lokacija,
                Statusvoznjeid = 320,
                Statusvoznje = status,
                DatumIVrijemePolaska = new DateTime(2023, 12, 1),
                DatumIVrijemeDolaska = new DateTime(2023, 12, 1).AddHours(2),
                BrojPutnika = 1
            };

            context.Troskovis.Add(troskovi);
            context.Lokacijas.Add(lokacija);
            context.Statusvoznjes.Add(status);
            context.Oglasvoznjas.Add(ad);
            await context.SaveChangesAsync();

            var controller = new OglasVoznjaController(context);
            var actionResult = await controller.ObrisiOglasVoznje(1020);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<Oglasvoznja>(ok.Value);

            Assert.Equal(1020, returned.Idoglasvoznja);
            Assert.Null(await context.Oglasvoznjas.FindAsync(1020));
            Assert.Null(await context.Troskovis.FindAsync(120));
            Assert.Null(await context.Lokacijas.FindAsync(220));
            Assert.Null(await context.Statusvoznjes.FindAsync(320));
        }
    }
}
