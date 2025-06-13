// CARSHARE_WEBAPP.Tests.Unit/VoziloControllerTests.cs
#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{
    public class VoziloControllerTests : IDisposable
    {
        private readonly CarshareContext _db;
        private readonly VoziloController _sut;

        private sealed class InMemoryCarshareContext : CarshareContext
        {
            public InMemoryCarshareContext(DbContextOptions<CarshareContext> opts) : base(opts) { }
            protected override void OnConfiguring(DbContextOptionsBuilder _) {  }
        }

        public VoziloControllerTests()
        {
            var opts = new DbContextOptionsBuilder<CarshareContext>()
                       .UseInMemoryDatabase(Guid.NewGuid().ToString())
                       .Options;

            _db = new InMemoryCarshareContext(opts);
            SeedDatabase(_db);

            _sut = new VoziloController(_db);
        }

        [Fact]
        public async Task GetVehicles_returns_all_vehicles_descending()
        {
            var result = await _sut.GetVehicles();
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var json = JsonSerializer.Serialize(ok.Value);
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement;

            Assert.Equal(2, arr.GetArrayLength());

            int first = arr[0].GetProperty("Idvozilo").GetInt32();
            int second = arr[1].GetProperty("Idvozilo").GetInt32();

            Assert.True(first > second);   
        }

        [Fact]
        public async Task GetVehicleById_existingId_returns_OK_with_projection()
        {
            var res = await _sut.GetVehicleById(1);
            var ok = Assert.IsType<OkObjectResult>(res.Result);

            var json = JsonSerializer.Serialize(ok.Value);
            using var doc = JsonDocument.Parse(json);
            var elem = doc.RootElement;

            Assert.Equal("Golf", elem.GetProperty("Naziv").GetString());
            Assert.Equal("VW", elem.GetProperty("Marka").GetString());
            Assert.Equal(2, elem.GetProperty("Idkorisnik").GetInt32());
        }

        [Fact]
        public async Task GetVehicleById_nonExisting_returns_NotFound()
        {
            var res = await _sut.GetVehicleById(999);
            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Details_existingId_maps_to_full_DTO()
        {
            var res = await _sut.Details(1);
            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var dto = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.Equal(1, dto.Idvozilo);
            Assert.Equal("Mark", dto.Vozac.Ime);
        }

        [Fact]
        public async Task KrerirajVozilo_creates_and_returns_identity()
        {
            var dto = new VoziloDTO
            {
                Naziv = "Punto",
                Marka = "Fiat",
                Model = "1.2",
                Registracija = "ZG-123-GG",
                VozacId = 2
            };

            var res = await _sut.KrerirajVozilo(dto);
            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var back = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.NotEqual(0, back.Idvozilo);
            Assert.Equal("Punto", back.Naziv);
            Assert.True(await _db.Vozilos.AnyAsync(v => v.Naziv == "Punto"));
        }

        [Fact]
        public async Task AcceptOrDenyVehicle_sets_IsConfirmed_flag()
        {
            var dto = new PotvrdaVoziloDTO { Id = 1, IsConfirmed = true };

            var res = await _sut.AcceptOrDenyVehicle(dto);
            var ok = Assert.IsType<OkObjectResult>(res);

            Assert.Equal("Vehicle was successfully confirmed", ok.Value);

            var entity = await _db.Vozilos.FindAsync(1);
            Assert.True(entity.Isconfirmed);
        }

        private static void SeedDatabase(CarshareContext db)
        {
            var driver = new Korisnik
            {
                Idkorisnik = 2,
                Ime = "Mark",
                Prezime = "Driver",
                Username = "markd",
                Email = "mark@example.com",
                Pwdhash = "dummyHash",
                Pwdsalt = "dummySalt"
            };

            var car1 = new Vozilo
            {
                Idvozilo = 1,
                Naziv = "Golf",
                Marka = "VW",
                Model = "VII",
                Registracija = "ZG-123-AB",
                Vozacid = 2,
                Isconfirmed = false,
                Vozac = driver
            };
            var car2 = new Vozilo
            {
                Idvozilo = 5,
                Naziv = "Octavia",
                Marka = "Škoda",
                Model = "III",
                Registracija = "ZG-555-CD",
                Vozacid = 2,
                Isconfirmed = true,
                Vozac = driver
            };

            db.Korisniks.Add(driver);
            db.Vozilos.AddRange(car1, car2);

            db.Images.Add(new Image
            {
                Idimage = 100,
                Name = "licence_front.jpg",
                Content = new byte[] { 0xAA },
                Imagetypeid = 4
            });

            db.SaveChanges();
        }

        public void Dispose() => _db.Dispose();
    }
}
