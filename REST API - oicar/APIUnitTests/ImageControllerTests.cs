using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{
    public class TestCarshareContext : CarshareContext
    {
        public TestCarshareContext(DbContextOptions<CarshareContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    }

    public class ImageControllerTests : IDisposable
    {
        private readonly TestCarshareContext _context;
        private readonly ImageController _controller;

        public ImageControllerTests()
        {
            var options = new DbContextOptionsBuilder<CarshareContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new TestCarshareContext(options);
            _controller = new ImageController(_context);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task DisplayImage_NotFound()
        {
            var result = await _controller.DisplayImage(1);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DisplayImage_Found_ReturnsDto()
        {
            var type = new Imagetype { Idimagetype = 1, Name = "t" };
            var img = new Image
            {
                Idimage = 2,
                Name = "n",
                Content = new byte[] { 1, 2, 3 },
                Imagetypeid = 1,
                Imagetype = type
            };
            _context.Imagetypes.Add(type);
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var result = await _controller.DisplayImage(2);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ImageDisplayDTO>(ok.Value);

            Assert.Equal("n", dto.Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 1, 2, 3 }), dto.Content);
            Assert.Equal("t", dto.Type);
        }

        [Fact]
        public async Task GetImagesForUser_NoImages_ReturnsEmpty()
        {
            var result = await _controller.GetImagesForUser(5);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<ImageDisplayDTO>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetImagesForUser_WithImages_ReturnsDtos()
        {
            var type = new Imagetype { Idimagetype = 1, Name = "xt" };
            var img = new Image
            {
                Idimage = 3,
                Name = "i3",
                Content = new byte[] { 4, 5 },
                Imagetypeid = 1,
                Imagetype = type
            };
            var ki = new Korisnikimage { Korisnikid = 10, Image = img };
            _context.Imagetypes.Add(type);
            _context.Images.Add(img);
            _context.Korisnikimages.Add(ki);
            await _context.SaveChangesAsync();

            var result = await _controller.GetImagesForUser(10);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<ImageDisplayDTO>>(ok.Value);

            Assert.Single(list);
            Assert.Equal("i3", list[0].Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 4, 5 }), list[0].Content);
            Assert.Equal("xt", list[0].Type);
        }

        [Fact]
        public async Task GetImagesForVehicle_VehicleNotFound()
        {
            var result = await _controller.GetImagesForVehicle(7);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetImagesForVehicle_NoMatching_ReturnsEmpty()
        {
            var v = new Vozilo
            {
                Idvozilo = 8,
                Registracija = "AB",
                Marka = "X",    // non-nullable
                Model = "Y"     // non-nullable
            };
            _context.Vozilos.Add(v);
            await _context.SaveChangesAsync();

            var result = await _controller.GetImagesForVehicle(8);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<ImageDisplayDTO>>(ok.Value);
            Assert.Empty(list);
        }



        [Fact]
        public async Task GetImages_ReturnsAll()
        {
            var img = new Image
            {
                Idimage = 6,
                Name = "all",
                Content = new byte[] { 7, 8 },
                Imagetypeid = 0
            };
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var result = await _controller.GetImages();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<ImageUploadDTO>>(ok.Value);

            Assert.Single(list);
            Assert.Equal("all", list[0].Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 7, 8 }), list[0].Base64Content);
        }

        [Fact]
        public async Task GetImageById_NotFound()
        {
            var result = await _controller.GetImageById(11);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetImageById_Found_ReturnsDto()
        {
            var img = new Image
            {
                Idimage = 12,
                Name = "byid",
                Content = new byte[] { 1 },
                Imagetypeid = 0
            };
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var result = await _controller.GetImageById(12);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ImageUploadDTO>(ok.Value);

            Assert.Equal("byid", dto.Name);
        }

        [Fact]
        public async Task UploadImage_EmptyContent_ReturnsBadRequest()
        {
            var dto = new ImageUploadDTO { Name = "n", Base64Content = "" };
            var result = await _controller.UploadImage(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Base64 content is required.", bad.Value);
        }

        [Fact]
        public async Task UploadImage_InvalidBase64_ReturnsBadRequest()
        {
            var dto = new ImageUploadDTO { Name = "x", Base64Content = "!!!" };
            var result = await _controller.UploadImage(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Invalid Base64 string.", bad.Value);
        }

        [Fact]
        public async Task UploadImage_Valid_ReturnsId()
        {
            var bytes = Encoding.UTF8.GetBytes("ok");
            var dto = new ImageUploadDTO { Name = "nm", Base64Content = Convert.ToBase64String(bytes), ImageTypeId = 0 };
            var result = await _controller.UploadImage(dto);
            var okRes = Assert.IsType<OkObjectResult>(result.Result);

            Assert.IsType<int>(okRes.Value);
        }

        [Fact]
        public async Task UpdateImage_NotFound()
        {
            var dto = new ImageUploadDTO { Name = "u", Base64Content = Convert.ToBase64String(new byte[] { 1 }) };
            var result = await _controller.UpdateImage(100, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateImage_InvalidBase64_ReturnsBadRequest()
        {
            var img = new Image
            {
                Idimage = 13,
                Name = "a",
                Content = new byte[] { 0 },
                Imagetypeid = 0
            };
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var dto = new ImageUploadDTO { Name = "a", Base64Content = "??" };
            var bad = Assert.IsType<BadRequestObjectResult>(await _controller.UpdateImage(13, dto));

            Assert.Equal("Invalid Base64 string.", bad.Value);
        }

        [Fact]
        public async Task UpdateImage_Valid_ReturnsDto()
        {
            var img = new Image
            {
                Idimage = 14,
                Name = "old",
                Content = new byte[] { 2 },
                Imagetypeid = 0
            };
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var dto = new ImageUploadDTO { Name = "new", Base64Content = Convert.ToBase64String(new byte[] { 3 }) };
            var ok = Assert.IsType<OkObjectResult>(await _controller.UpdateImage(14, dto));
            var returned = Assert.IsType<ImageUploadDTO>(ok.Value);

            Assert.Equal("new", returned.Name);
        }

        [Fact]
        public async Task DeleteImage_NotFound()
        {
            var result = await _controller.DeleteImage(200);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteImage_Success_ReturnsNoContent()
        {
            var img = new Image
            {
                Idimage = 15,
                Name = "del",
                Content = new byte[] { 4 },
                Imagetypeid = 0
            };
            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            var noContent = Assert.IsType<NoContentResult>(await _controller.DeleteImage(15));
        }
    }
}
