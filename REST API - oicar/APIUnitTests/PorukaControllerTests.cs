using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using REST_API___oicar.Security;
using Xunit;

namespace APIUnitTests
{
    public class PorukaControllerTests : IDisposable
    {
        private readonly TestCarshareContext _ctx;
        private readonly PorukaController _ctrl;
        private readonly AesEncryptionService _encryptionService;

        public PorukaControllerTests(AesEncryptionService encryptionService) 
        { 
            var opts = new DbContextOptionsBuilder<CarshareContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _ctx = new TestCarshareContext(opts);
            _ctrl = new PorukaController(_ctx, encryptionService); 
        } 

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public async Task GetMessagesForRide_NoMessages_ReturnsEmpty()
        { 
            var result = await _ctrl.GetMessagesForRide(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<PorukaGetDTO>>(ok.Value);
            Assert.Empty(list);
        } 

        [Fact]
        public async Task GetMessagesForRide_WithMessages_ReturnsOrderedDtos()
        { 
            var ov = new Oglasvoznja { Idoglasvoznja = 10 };
            _ctx.Oglasvoznjas.Add(ov);

            var putnik = new Korisnik
            {
                Idkorisnik = 1,
                Username = "P",
                Ime = "ImeP",
                Prezime = "PrezP",
                Email = "e@p.t",
                Telefon = "000",
                Pwdhash = "h",
                Pwdsalt = "s",
                Datumrodjenja = DateOnly.FromDateTime(DateTime.Today),
                Ulogaid = 1
            };

            var msg1 = new Poruka
            {
                Idporuka = 1,
                Oglasvoznjaid = 10,
                Putnikid = 1,
                Content = "Hello",
                Putnik = putnik
            };

            var vozac = new Korisnik
            {
                Idkorisnik = 2,
                Username = "V",
                Ime = "ImeV",
                Prezime = "PrezV",
                Email = "e@v.t",
                Telefon = "111",
                Pwdhash = "h2",
                Pwdsalt = "s2",
                Datumrodjenja = DateOnly.FromDateTime(DateTime.Today),
                Ulogaid = 2
            };

            var msg2 = new Poruka
            {
                Idporuka = 2,
                Oglasvoznjaid = 10,
                Vozacid = 2,
                Content = "World",
                Vozac = vozac
            };

            var msg3 = new Poruka
            {
                Idporuka = 3,
                Oglasvoznjaid = 10,
                Content = "?"
            };

            _ctx.Korisniks.AddRange(putnik, vozac);
            _ctx.Porukas.AddRange(msg1, msg2, msg3);
            await _ctx.SaveChangesAsync();

            var result = await _ctrl.GetMessagesForRide(10);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<PorukaGetDTO>>(ok.Value);

            Assert.Equal(3, list.Count);
            Assert.Equal("P", list[0].SenderName);
            Assert.Equal("World", list[1].Content);
            Assert.Equal("V", list[1].SenderName);
            Assert.Equal("Unknown user", list[2].SenderName);
        }

        [Theory]
        [InlineData(0, null)]
        [InlineData(5, "")]
        public async Task SendMessageForRide_MissingFields_ReturnsBadRequest(int kvId, string content)
        {
            var dto = new PorukaSendDTO
            {
                OglasVoznjaId = kvId,
                Content = content,
                PutnikId = 1
            };
            var bad = Assert.IsType<BadRequestObjectResult>(await _ctrl.SendMessageForRide(dto));
            Assert.Contains("OglasVoznjaId i sadržaj poruke", bad.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_BothIdsSet_ReturnsBadRequest()
        {
            var dto = new PorukaSendDTO
            {
                OglasVoznjaId = 1,
                Content = "Hi",
                PutnikId = 1,
                VozacId = 2
            };
            var bad = Assert.IsType<BadRequestObjectResult>(await _ctrl.SendMessageForRide(dto));
            Assert.Contains("točno jedan korisnik", bad.Value.ToString());
        }

        [Fact]
        public async Task SendMessageForRide_RideNotFound_ReturnsNotFound()
        {
            var dto = new PorukaSendDTO
            {
                OglasVoznjaId = 100,
                Content = "Hi",
                PutnikId = 1
            };
            var nf = Assert.IsType<NotFoundObjectResult>(await _ctrl.SendMessageForRide(dto));
            Assert.Contains("Vožnja ne postoji", nf.Value.ToString());
        }

        [Fact]
        public async Task GetMessagesForVehicle_MirrorsRideLogic()
        {
            var kv = new Oglasvoznja { Idoglasvoznja = 40 };
            var msg = new Poruka
            {
                Idporuka = 5,
                Oglasvoznjaid = 40,
                Content = "Veh"
            };
            _ctx.Oglasvoznjas.Add(kv);
            _ctx.Porukas.Add(msg);
            await _ctx.SaveChangesAsync();

            var r = await _ctrl.GetMessagesForVehicle(40);
            var ok = Assert.IsType<OkObjectResult>(r.Result);
            var list = Assert.IsType<List<PorukaGetDTO>>(ok.Value);
            Assert.Single(list);
            Assert.Equal("Veh", list[0].Content);
        }
        
        [Fact]
            public async Task SendMessageForVehicle_ForbiddenIfNotParticipant()
            {
                var vozilo = new Vozilo { Idvozilo = 100, Vozacid = 7 };
                _ctx.Vozilos.Add(vozilo); 

                var oglas = new Oglasvoznja { Idoglasvoznja = 50, Voziloid = 100, Vozilo = vozilo };
                _ctx.Oglasvoznjas.Add(oglas);
                await _ctx.SaveChangesAsync();

                var dto1 = new PorukaSendDTO { OglasVoznjaId = 50, Content = "X", PutnikId = 9 };
                Assert.IsType<ForbidResult>(await _ctrl.SendMessageForVehicle(dto1));

                var dto2 = new PorukaSendDTO { OglasVoznjaId = 50, Content = "X", VozacId = 9 };
                Assert.IsType<ForbidResult>(await _ctrl.SendMessageForVehicle(dto2));
            } 

            [Fact] 
            public async Task SendMessageForVehicle_Valid_SavesAndReturnsOk()
            { 
                var vozilo = new Vozilo { Idvozilo = 200, Vozacid = 9 };
                _ctx.Vozilos.Add(vozilo);

                var oglas = new Oglasvoznja { Idoglasvoznja = 80, Voziloid = 200, Vozilo = vozilo };
                _ctx.Oglasvoznjas.Add(oglas);
                await _ctx.SaveChangesAsync();

                var dto = new PorukaSendDTO
                {
                    OglasVoznjaId = 80,
                    Content = "OK",
                    VozacId = 9
                };

                var ok = Assert.IsType<OkObjectResult>(await _ctrl.SendMessageForVehicle(dto));
                var saved = _ctx.Porukas.Single();
                Assert.Equal("OK", saved.Content);
                Assert.Equal(9, saved.Vozacid); 
            }
    }
}
