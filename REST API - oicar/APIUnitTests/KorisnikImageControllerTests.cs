using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{


    public class KorisnikImageControllerTests : IDisposable
    {
        private readonly TestCarshareContext _ctx;
        private readonly KorisnikImageController _ctrl;

        public KorisnikImageControllerTests()
        {
            var opts = new DbContextOptionsBuilder<CarshareContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _ctx = new TestCarshareContext(opts);
            _ctrl = new KorisnikImageController(_ctx);
        }

        public void Dispose() => _ctx.Dispose();

        [Fact]
        public void Get_NoParameters_ReturnsOk()
        {
            var action = _ctrl.Get();
            Assert.IsType<OkResult>(action.Result);
        }

        [Fact]
        public void Get_WithId_ReturnsValue()
        {
            var result = _ctrl.Get(5);
            Assert.Equal("value", result);
        }

        [Fact]
        public void CreateKorisnikImage_AddsToContextAndReturnsOk()
        {
            var dto = new KorisnikImageDTO { KorisnikId = 7, ImageId = 3 };
            var action = _ctrl.CreateKorisnikImage(dto);
            Assert.IsType<OkResult>(action.Result);

            var saved = _ctx.Korisnikimages.Single();
            Assert.Equal(7, saved.Korisnikid);
            Assert.Equal(3, saved.Imageid);
        }
    }
}
