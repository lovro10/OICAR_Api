using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using Xunit;

namespace APIUnitTests
{
    public class VoziloControllerTests
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

        //[Fact]
        //public async Task GetVehicles_ReturnsProjectedList()
        //{
        //    var context = CreateInMemoryContext(Guid.NewGuid().ToString());

        //    var vehicle1 = new Vozilo
        //    {
        //        Idvozilo = 1,
        //        Marka = "MarkaA",
        //        Model = "ModelA",
        //        Registracija = "REGA",
        //        Naziv = "NameA",
        //        Isconfirmed = true
        //    };
        //    var vehicle2 = new Vozilo
        //    {
        //        Idvozilo = 2,
        //        Marka = "MarkaB",
        //        Model = "ModelB",
        //        Registracija = "REGB",
        //        Naziv = "NameB",
        //        Isconfirmed = false
        //    };
        //    context.Vozilos.AddRange(vehicle1, vehicle2);
        //    await context.SaveChangesAsync();

        //    var controller = new VoziloController(context);
        //    var actionResult = await controller.GetVehicles();
        //    var ok = Assert.IsType<OkObjectResult>(actionResult.Result);

        //    var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
        //    var items = list.ToList();
        //    Assert.Equal(2, items.Count);

        //    var first = items[0];
        //    var firstType = first.GetType();
        //    var firstId = (int)firstType.GetProperty("Idvozilo").GetValue(first);
        //    Assert.Equal(2, firstId);
        //}

        [Fact]
        public async Task GetVehicleById_Existing_ReturnsProjectedObject()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var driver = new Korisnik
            {
                Idkorisnik = 5,
                Username = "drvUser",
                Ime = "Driver",
                Prezime = "D",
                Email = "d@example.com",
                Telefon = "123",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };

            var vehicle = new Vozilo
            {
                Idvozilo = 10,
                Marka = "MarkaX",
                Model = "ModelX",
                Registracija = "REGX",
                Isconfirmed = true,
                Vozacid = 5,
                Vozac = driver
            };

            context.Korisniks.Add(driver);
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new VoziloController(context);
            var actionResult = await controller.GetVehicleById(10);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);

            var obj = ok.Value;
            var type = obj.GetType();
            var idProp = type.GetProperty("Idvozilo").GetValue(obj);
            Assert.Equal(10, idProp);
        }

        [Fact]
        public async Task GetVehicleById_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);

            var actionResult = await controller.GetVehicleById(999);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Details_Existing_ReturnsVoziloDTO()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var vehicle = new Vozilo
            {
                Idvozilo = 20,
                Marka = "MarkaY",
                Model = "ModelY",
                Registracija = "REGY"
            };
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new VoziloController(context);
            var actionResult = await controller.Details(20);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dto = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.Equal(20, dto.Idvozilo);
        }

        [Fact]
        public async Task Details_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);

            var actionResult = await controller.Details(999);
            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Contains("nije pronađeno", notFound.Value.ToString());
        }

        [Fact]
        public void GetVehicleByUser_ReturnsOnlyThatUserVehicles()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var user1 = new Korisnik
            {
                Idkorisnik = 1,
                Username = "user1",
                Ime = "U1",
                Prezime = "One",
                Email = "u1@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var user2 = new Korisnik
            {
                Idkorisnik = 2,
                Username = "user2",
                Ime = "U2",
                Prezime = "Two",
                Email = "u2@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1991, 2, 2),
                Pwdsalt = "",
                Pwdhash = ""
            };
            var vehicle1 = new Vozilo
            {
                Idvozilo = 30,
                Vozacid = 1,
                Naziv = "V1",
                Marka = "Mk1",
                Model = "Md1",
                Registracija = "REG1",
                Isconfirmed = true
            };
            var vehicle2 = new Vozilo
            {
                Idvozilo = 31,
                Vozacid = 2,
                Naziv = "V2",
                Marka = "Mk2",
                Model = "Md2",
                Registracija = "REG2",
                Isconfirmed = false
            };
            context.Korisniks.AddRange(user1, user2);
            context.Vozilos.AddRange(vehicle1, vehicle2);
            context.SaveChanges();

            var controller = new VoziloController(context);
            var actionResult = controller.GetVehicleByUser(userId: 1);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<VoziloDTO>>(ok.Value);
            var arr = list.ToList();

            Assert.Single(arr);
            Assert.Equal(30, arr[0].Idvozilo);
        }

        [Fact]
        public async Task KrerirajVozilo_Success_ReturnsDtoWithId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var controller = new VoziloController(context);
            var dto = new VoziloDTO
            {
                Naziv = "NewVehicle",
                Marka = "MkNew",
                Model = "MdNew",
                Registracija = "REGNEW",
                VozacId = 5
            };
            var actionResult = await controller.KrerirajVozilo(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.True(returned.Idvozilo > 0);
            Assert.Equal("MkNew", returned.Marka);
        }

        [Fact]
        public async Task CreateVehicle_MissingImages_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);

            var dto = new VoziloDTO
            {
                Naziv = "VNoImg",
                Marka = "MkNI",
                Model = "MdNI",
                Registracija = "REGNI",
                VozacId = 7,
                FrontImageBase64 = "",
                BackImageBase64 = ""
            };

            var actionResult = await controller.CreateVehicle(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Contains("Both front and back images are required", bad.Value.ToString());
        }

        [Fact]
        public async Task CreateVehicle_Success_CreatesImagesAndVehicle()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var dto = new VoziloDTO
            {
                Naziv = "VWithImg",
                Marka = "MkWI",
                Model = "MdWI",
                Registracija = "REGWI",
                VozacId = 8,
                FrontImageBase64 = base64,
                BackImageBase64 = base64,
                FrontImageName = "front.png",
                BackImageName = "back.png"
            };

            var controller = new VoziloController(context);
            var actionResult = await controller.CreateVehicle(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var returned = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.True(returned.Idvozilo > 0);
        }

        [Fact]
        public async Task UpdateVehicle_Success_ReturnsDtoWithUpdatedFields()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var vehicle = new Vozilo
            {
                Idvozilo = 50,
                Marka = "OldMk",
                Model = "OldMd",
                Registracija = "OLDREG"
            };
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new VoziloController(context);
            var updatedDto = new VoziloDTO
            {
                Marka = "NewMk",
                Model = "NewMd",
                Registracija = "NEWREG"
            };
            var json = JsonConvert.SerializeObject(updatedDto);
            var actionResult = await controller.UpdateVehicle(50, json);
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<VoziloDTO>(ok.Value);

            Assert.Equal(50, returned.Idvozilo);
        }

        [Fact]
        public async Task UpdateVehicle_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);
            var dto = new VoziloDTO { Marka = "X", Model = "Y", Registracija = "Z" };
            var json = JsonConvert.SerializeObject(dto);

            var actionResult = await controller.UpdateVehicle(999, json);
            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Contains("nije pronađeno", notFound.Value.ToString());
        }

        [Fact]
        public async Task DeleteVehicle_Existing_ReturnsDeletedEntity()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var vehicle = new Vozilo
            {
                Idvozilo = 60,
                Marka = "MkDel",
                Model = "MdDel",
                Registracija = "REGDEL"
            };
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new VoziloController(context);
            var actionResult = await controller.DeleteVehicle(60);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var deleted = Assert.IsType<Vozilo>(ok.Value);

            Assert.Equal(60, deleted.Idvozilo);
        }

        [Fact]
        public async Task DeleteVehicle_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);

            var actionResult = await controller.DeleteVehicle(999);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task AcceptOrDenyVehicle_Existing_ReturnsConfirmationMessage()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var vehicle = new Vozilo
            {
                Idvozilo = 70,
                Marka = "MkAcc",
                Model = "MdAcc",
                Registracija = "REGACC",
                Isconfirmed = null
            };
            context.Vozilos.Add(vehicle);
            await context.SaveChangesAsync();

            var controller = new VoziloController(context);
            var dto = new PotvrdaVoziloDTO
            {
                Id = 70,
                IsConfirmed = true
            };
            var actionResult = await controller.AcceptOrDenyVehicle(dto);
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var message = Assert.IsType<string>(ok.Value);

            Assert.Contains("confirmed", message);
        }

        [Fact]
        public async Task AcceptOrDenyVehicle_NotExisting_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new VoziloController(context);

            var dto = new PotvrdaVoziloDTO
            {
                Id = 999,
                IsConfirmed = false
            };
            var actionResult = await controller.AcceptOrDenyVehicle(dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.Contains("Vehicle was not found", notFound.Value.ToString());
        }
    }
}
