using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{


    public class KorisnikVoznjaControllerTests : IDisposable
    {
        private readonly TestCarshareContext _ctx;
        private readonly KorisnikVoznjaController _ctrl;

        public KorisnikVoznjaControllerTests()
        {
            var opts = new DbContextOptionsBuilder<CarshareContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _ctx = new TestCarshareContext(opts);
            _ctrl = new KorisnikVoznjaController(_ctx);
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public async Task GetAll_Empty_ReturnsEmptyList()
        {
            var action = await _ctrl.GetAll();
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsType<List<KorisnikVoznjaDTO>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetAll_WithData_ReturnsDtos()
        {
            var korisnik = new Korisnik
            {
                Idkorisnik = 1,
                Ime = "i",
                Prezime = "p",
                Email = "e",
                Telefon = "t",
                Username = "u",
                Pwdhash = "h",
                Pwdsalt = "s",
                Datumrodjenja = DateOnly.FromDateTime(DateTime.Today),
                Ulogaid = 1
            };
            var lok = new Lokacija { Idlokacija = 1, Polaziste = "A", Odrediste = "B" };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 2,
                Lokacija = lok,
                Korisnikvoznjas = new List<Korisnikvoznja>(),
                BrojPutnika = 0,
                DatumIVrijemePolaska = DateTime.Now
            };
            var kv = new Korisnikvoznja
            {
                Idkorisnikvoznja = 3,
                Korisnikid = 1,
                Oglasvoznjaid = 2,
                Korisnik = korisnik,
                Oglasvoznja = oglas
            };

            _ctx.Korisniks.Add(korisnik);
            _ctx.Lokacijas.Add(lok);
            _ctx.Oglasvoznjas.Add(oglas);
            _ctx.Korisnikvoznjas.Add(kv);
            await _ctx.SaveChangesAsync();

            var action = await _ctrl.GetAll();
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsType<List<KorisnikVoznjaDTO>>(ok.Value);

            Assert.Single(list);
            Assert.Equal(3, list[0].IdKorisnikVoznja);
            Assert.Equal("A", list[0].LokacijaPutnik);
            Assert.Equal("B", list[0].LokacijaVozac);
        }

        [Fact]
        public async Task GetByUserAndRide_NotFound()
        {
            var action = await _ctrl.GetByUserAndRide(1, 2);
            Assert.IsType<NotFoundResult>(action.Result);
        }

        [Fact]
        public async Task GetByUserAndRide_Found_ReturnsDto()
        {
            var kv = new Korisnikvoznja
            {
                Idkorisnikvoznja = 4,
                Korisnikid = 5,
                Oglasvoznjaid = 6,
                Lokacijaputnik = "P",
                Lokacijavozac = "V"
            };
            _ctx.Korisnikvoznjas.Add(kv);
            await _ctx.SaveChangesAsync();

            var action = await _ctrl.GetByUserAndRide(5, 6);
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var dto = Assert.IsType<KorisnikVoznjaDTO>(ok.Value);
            Assert.Equal(4, dto.IdKorisnikVoznja);
        }

        [Fact]
        public async Task JoinRide_VoznjaNotFound_ReturnsNotFound()
        {
            var dto = new KorisnikVoznjaDTO { KorisnikId = 1, OglasVoznjaId = 2 };
            var result = await _ctrl.JoinRide(dto);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task JoinRide_AlreadyJoined_ReturnsBadRequest()
        {
            var lok = new Lokacija { Idlokacija = 2, Polaziste = "X", Odrediste = "Y" };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 7,
                BrojPutnika = 0,
                Lokacija = lok,
                Korisnikvoznjas = new List<Korisnikvoznja>
                {
                    new Korisnikvoznja { Korisnikid = 8, Oglasvoznjaid = 7 }
                }
            };
            _ctx.Lokacijas.Add(lok);
            _ctx.Oglasvoznjas.Add(oglas);
            await _ctx.SaveChangesAsync();

            var dto = new KorisnikVoznjaDTO { KorisnikId = 8, OglasVoznjaId = 7 };
            var result = await _ctrl.JoinRide(dto);
            var br = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("već prijavljen", br.Value.ToString());
        }

        [Fact]
        public async Task JoinRide_Full_ReturnsBadRequest()
        {
            var lok = new Lokacija { Idlokacija = 3, Polaziste = "X", Odrediste = "Y" };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 9,
                BrojPutnika = 0,
                Lokacija = lok,
                Korisnikvoznjas = new List<Korisnikvoznja> { new(), new() }
            };
            _ctx.Lokacijas.Add(lok);
            _ctx.Oglasvoznjas.Add(oglas);
            await _ctx.SaveChangesAsync();

            var dto = new KorisnikVoznjaDTO { KorisnikId = 1, OglasVoznjaId = 9 };
            var result = await _ctrl.JoinRide(dto);
            var br = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Nema slobodnih mjesta", br.Value.ToString());
        }

        [Fact]
        public async Task JoinRide_Success_ReturnsDtoWithId()
        {
            var lok = new Lokacija { Idlokacija = 4, Polaziste = "S", Odrediste = "D" };
            var oglas = new Oglasvoznja
            {
                Idoglasvoznja = 10,
                BrojPutnika = 1,
                Lokacija = lok,
                Korisnikvoznjas = new List<Korisnikvoznja>()
            };
            _ctx.Lokacijas.Add(lok);
            _ctx.Oglasvoznjas.Add(oglas);
            await _ctx.SaveChangesAsync();

            var dto = new KorisnikVoznjaDTO { KorisnikId = 2, OglasVoznjaId = 10 };
            var ok = Assert.IsType<OkObjectResult>(await _ctrl.JoinRide(dto));
            var returned = Assert.IsType<KorisnikVoznjaDTO>(ok.Value);
            Assert.True(returned.IdKorisnikVoznja > 0);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task UserJoinedRide_InvalidIds_ReturnsBadRequest(int u, int o)
        {
            var br = Assert.IsType<BadRequestObjectResult>(await _ctrl.UserJoinedRide(u, o));
            Assert.False((bool)br.Value);
        }

        [Fact]
        public async Task UserJoinedRide_Passenger_ReturnsTrue()
        {
            _ctx.Korisnikvoznjas.Add(new Korisnikvoznja { Korisnikid = 3, Oglasvoznjaid = 4 });
            await _ctx.SaveChangesAsync();

            var ok = Assert.IsType<OkObjectResult>(await _ctrl.UserJoinedRide(3, 4));
            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task UserJoinedRide_Driver_ReturnsTrue()
        {
            var voz = new Vozilo
            {
                Vozacid = 6,
                Marka = "M",
                Model = "D",
                Registracija = "R"
            };
            var ride = new Oglasvoznja
            {
                Idoglasvoznja = 5,
                Vozilo = voz
            };
            _ctx.Vozilos.Add(voz);
            _ctx.Oglasvoznjas.Add(ride);
            await _ctx.SaveChangesAsync();

            var ok = Assert.IsType<OkObjectResult>(await _ctrl.UserJoinedRide(6, 5));
            Assert.True((bool)ok.Value);
        }

        [Fact]
        public async Task UserJoinedRide_None_ReturnsFalse()
        {
            var ok = Assert.IsType<OkObjectResult>(await _ctrl.UserJoinedRide(7, 8));
            Assert.False((bool)ok.Value);
        }

        [Fact]
        public async Task GetHistoryOfRides_OnlyPast_ReturnsDtos()
        {
            var now = DateTime.Now;
            var lok = new Lokacija { Idlokacija = 7, Polaziste = "P", Odrediste = "D" };
            _ctx.Lokacijas.Add(lok);
            var past = new Korisnikvoznja
            {
                Idkorisnikvoznja = 11,
                Korisnikid = 9,
                Oglasvoznja = new Oglasvoznja { DatumIVrijemePolaska = now.AddDays(-1), Lokacija = lok }
            };
            var future = new Korisnikvoznja
            {
                Idkorisnikvoznja = 12,
                Korisnikid = 9,
                Oglasvoznja = new Oglasvoznja { DatumIVrijemePolaska = now.AddDays(1), Lokacija = lok }
            };
            _ctx.Korisnikvoznjas.AddRange(past, future);
            await _ctx.SaveChangesAsync();

            var action = await _ctrl.GetHistoryOfRides(9);
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsType<List<VoznjaHistoryDTO>>(ok.Value);
            Assert.Single(list);
            Assert.Equal(11, list[0].Korisnikvoznjaid);
        }

        [Fact]
        public async Task DeleteKorisnikVoznja_NotFound_ReturnsNotFound()
        {
            var result = await _ctrl.DeleteKorisnikVoznja(1, 2);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteKorisnikVoznja_Found_DeletesAndReturnsEntity()
        {
            var kv = new Korisnikvoznja { Korisnikid = 2, Oglasvoznjaid = 3 };
            _ctx.Korisnikvoznjas.Add(kv);
            await _ctx.SaveChangesAsync();

            var ok = Assert.IsType<OkObjectResult>(await _ctrl.DeleteKorisnikVoznja(2, 3));
            var returned = Assert.IsType<Korisnikvoznja>(ok.Value);
            Assert.Equal(kv.Korisnikid, returned.Korisnikid);
            Assert.Empty(_ctx.Korisnikvoznjas);
        }
    }
}
