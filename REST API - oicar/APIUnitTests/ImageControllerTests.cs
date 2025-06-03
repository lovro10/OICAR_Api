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
/*
 * unit testovi koji koriste InMemoryDB, trebalo je doraditi DbContext zbog konflikta 
 * da bi izdvojili testove od PostgreSQL
 * --------------------------------------------------------------------------------------
 * registriramo InMemory provider za EF core (AddEntityFrameworkInMemoryDatabase)
 * onda postavljamo da CarShareContext koritsi tu inmemory bazu te na kraju vraca novi carsharecontext koji
 * postoji samo u toj memoriji
 * 
 * zasto?
 * ne zelimo pristupati pravoj bazi i drugim providerima, izbjegavanje sukoba, brze testiranje
 * 
 */
namespace APIUnitTests
{
    public class ImageControllerTests
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
        public async Task DisplayImage_ImageExists_ReturnsOkWithDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var type = new Imagetype { Idimagetype = 1, Name = "JPG" };
            context.Imagetypes.Add(type);
            var image = new Image
            {
                Idimage = 1,
                Name = "TestImage",
                Content = new byte[] { 1, 2, 3 },
                Imagetypeid = 1,
                Imagetype = type
            };
            context.Images.Add(image);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.DisplayImage(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ImageDisplayDTO>(ok.Value);
            Assert.Equal("TestImage", dto.Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 1, 2, 3 }), dto.Content);
            Assert.Equal("JPG", dto.Type);
        }

        [Fact]
        public async Task DisplayImage_ImageNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var result = await controller.DisplayImage(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetImagesForUser_NoImages_ReturnsEmptyList()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var result = await controller.GetImagesForUser(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ImageDisplayDTO>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetImagesForUser_WithImages_ReturnsList()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var type = new Imagetype { Idimagetype = 1, Name = "PNG" };
            context.Imagetypes.Add(type);

            var image = new Image
            {
                Idimage = 1,
                Name = "UserImg",
                Content = new byte[] { 4, 5, 6 },
                Imagetypeid = 1,
                Imagetype = type
            };
            context.Images.Add(image);

            context.Korisnikimages.Add(new Korisnikimage
            {
                Idkorisnikimage = 1,
                Korisnikid = 42,
                Imageid = 1,
                Image = image
            });
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.GetImagesForUser(42);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ImageDisplayDTO>>(ok.Value);
            var dto = list.First();
            Assert.Equal("UserImg", dto.Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 4, 5, 6 }), dto.Content);
            Assert.Equal("PNG", dto.Type);
        }

        [Fact]
        public async Task GetImagesForVehicle_VehicleNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var result = await controller.GetImagesForVehicle(123);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetImagesForVehicle_WithImages_ReturnsEmptyListDueToCaseMismatch()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var vehicle = new Vozilo
            {
                Idvozilo = 7,
                Registracija = "ABC123",
                Marka = "TestMarka",
                Model = "TestModel"
            };
            context.Vozilos.Add(vehicle);

            var type = new Imagetype { Idimagetype = 2, Name = "JPG" };
            context.Imagetypes.Add(type);

            var img1 = new Image
            {
                Idimage = 1,
                Name = "PrednjaABC123",
                Content = new byte[] { 7, 8, 9 },
                Imagetypeid = 2,
                Imagetype = type
            };
            var img2 = new Image
            {
                Idimage = 2,
                Name = "StaraABC123",
                Content = new byte[] { 10, 11, 12 },
                Imagetypeid = 2,
                Imagetype = type
            };
            context.Images.AddRange(img1, img2);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.GetImagesForVehicle(7);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ImageDisplayDTO>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetImages_ReturnsAllImages()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var img1 = new Image { Idimage = 1, Name = "ImgOne", Content = new byte[] { 1 } };
            var img2 = new Image { Idimage = 2, Name = "ImgTwo", Content = new byte[] { 2 } };
            context.Images.AddRange(img1, img2);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.GetImages();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ImageUploadDTO>>(ok.Value);
            Assert.Contains(list, x => x.Name == "ImgOne");
            Assert.Contains(list, x => x.Name == "ImgTwo");
        }

        [Fact]
        public async Task GetImageById_Exists_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var img = new Image { Idimage = 5, Name = "SingleImg", Content = new byte[] { 3, 4 } };
            context.Images.Add(img);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.GetImageById(5);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ImageUploadDTO>(ok.Value);
            Assert.Equal("SingleImg", dto.Name);
            Assert.Equal(Convert.ToBase64String(new byte[] { 3, 4 }), dto.Base64Content);
        }

        [Fact]
        public async Task GetImageById_NotExists_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var result = await controller.GetImageById(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UploadImage_ValidBase64_ReturnsOkWithId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var type = new Imagetype { Idimagetype = 3, Name = "GIF" };
            context.Imagetypes.Add(type);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var dto = new ImageUploadDTO
            {
                Name = "NewImg",
                Base64Content = Convert.ToBase64String(new byte[] { 13, 14 }),
                ImageTypeId = 3
            };
            var result = await controller.UploadImage(dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var id = Assert.IsType<int>(ok.Value);
            var saved = await context.Images.FindAsync(id);
            Assert.NotNull(saved);
            Assert.Equal("NewImg", saved.Name);
        }

        [Fact]
        public async Task UploadImage_InvalidBase64_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var dto = new ImageUploadDTO
            {
                Name = "BadImg",
                Base64Content = "not_base64"
            };
            var result = await controller.UploadImage(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid Base64 string.", bad.Value);
        }

        [Fact]
        public async Task UpdateImage_Exists_ValidBase64_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var img = new Image
            {
                Idimage = 8,
                Name = "Old",
                Content = new byte[] { 15, 16 }
            };
            context.Images.Add(img);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var dto = new ImageUploadDTO
            {
                Name = "Updated",
                Base64Content = Convert.ToBase64String(new byte[] { 17, 18 })
            };
            var result = await controller.UpdateImage(8, dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            var updated = await context.Images.FindAsync(8);
            Assert.Equal("Updated", updated.Name);
            Assert.Equal(new byte[] { 17, 18 }, updated.Content);
        }

        [Fact]
        public async Task UpdateImage_NotExists_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var dto = new ImageUploadDTO
            {
                Name = "Nope",
                Base64Content = Convert.ToBase64String(new byte[] { 19 })
            };
            var result = await controller.UpdateImage(999, dto);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateImage_InvalidBase64_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var img = new Image
            {
                Idimage = 9,
                Name = "Img9",
                Content = new byte[] { 20 }
            };
            context.Images.Add(img);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var dto = new ImageUploadDTO
            {
                Name = "Img9",
                Base64Content = "!!!"
            };
            var result = await controller.UpdateImage(9, dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Base64 string.", bad.Value);
        }

        [Fact]
        public async Task DeleteImage_Exists_ReturnsNoContent()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var img = new Image
            {
                Idimage = 10,
                Name = "Img10",
                Content = new byte[] { 21 }
            };
            context.Images.Add(img);
            await context.SaveChangesAsync();

            var controller = new ImageController(context);
            var result = await controller.DeleteImage(10);
            Assert.IsType<NoContentResult>(result);
            var deleted = await context.Images.FindAsync(10);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteImage_NotExists_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new ImageController(context);
            var result = await controller.DeleteImage(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
