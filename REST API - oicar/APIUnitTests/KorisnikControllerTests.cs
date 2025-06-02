using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using REST_API___oicar.Controllers;
using REST_API___oicar.DTOs;
using REST_API___oicar.Models;
using REST_API___oicar.Security;
using Xunit;

namespace MyApi.Tests.Controllers
{
    public class KorisnikControllerTests
    {
        /*
         * Kreiramo novi inmemory CarShareContext da bi testove pokretali na izoliranoj bazi kako ne bi doslo do konflikata izmedju testova
         * 
         */
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
         /*
        Kreiramo inmemory IConfig sa JWTkeyem, treba nam jer kontroler cita JWT secret key
         */
        private IConfiguration CreateConfiguration()
        {
            var dict = new Dictionary<string, string>
            {
                { "Jwt:SecureKey", "TestSecureKey" }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }
        /*
         * Instanciramo Korisnika da bi proslo odredjene testove
         */
        private Korisnik MakeDummyUser(
            int id,
            string username,
            string? plainPassword = null)
        {
            string pwdHash, pwdSalt;
            if (plainPassword != null)
            {
                pwdSalt = PasswordHashProvider.GetSalt();
                pwdHash = PasswordHashProvider.GetHash(plainPassword, pwdSalt);
            }
            else
            {
                pwdSalt = "";
                pwdHash = "";
            }

            return new Korisnik
            {
                Idkorisnik = id,
                Username = username,
                Ime = "DummyIme",
                Prezime = "DummyPrezime",
                Email = "dummy@example.com",
                Telefon = "0000000000",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = pwdSalt,
                Pwdhash = pwdHash,
            };
        }

        [Fact]
        public async Task Update_UserExists_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 1,
                Ime = "Old",
                Prezime = "User",
                Email = "old@example.com",
                Telefon = "000",
                Datumrodjenja = new DateOnly(2000, 1, 1),
                Username = "olduser",
                Pwdsalt = "",
                Pwdhash = ""
            };
            context.Korisniks.Add(user);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikUpdateDTO
            {
                Ime = "NewName",
                Prezime = "NewLast",
                Email = "new@example.com",
                Telefon = "111",
                DatumRodjenja = new DateOnly(1999, 2, 2),
                Username = "newuser"
            };
            var result = await controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<KorisnikUpdateDTO>(ok.Value);
            Assert.Equal("NewName", returned.Ime);

            var updated = await context.Korisniks.FindAsync(1);
            Assert.Equal("newuser", updated.Username);
            Assert.Equal("new@example.com", updated.Email);
        }

        [Fact]
        public async Task Update_UserNotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikUpdateDTO
            {
                Ime = "Any",
                Prezime = "Any",
                Email = "any@example.com",
                Telefon = "123",
                DatumRodjenja = DateOnly.MinValue,
                Username = "anyuser"
            };
            var result = await controller.Update(999, dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("nije pronađen", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var role = new Uloga { Iduloga = 1, Naziv = "Admin" };
            context.Ulogas.Add(role);

            var user1 = new Korisnik
            {
                Idkorisnik = 1,
                Ime = "A",
                Prezime = "B",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Email = "a@example.com",
                Username = "usera",
                Pwdhash = "h",
                Pwdsalt = "s",
                Telefon = "111",
                Ulogaid = 1,
                Uloga = role,
                Isconfirmed = true
            };
            context.Korisniks.Add(user1);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var result = await controller.GetAll();
            var list = Assert.IsAssignableFrom<IEnumerable<KorisnikDTO>>(result.Value);
            Assert.Single(list);
            Assert.Equal("usera", list.First().Username);
        }

        [Fact]
        public async Task GetById_Exists_ReturnsUser()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 5,
                Username = "test",
                Ime = "X",
                Prezime = "Y",
                Email = "test@example.com",
                Telefon = "000",
                Datumrodjenja = new DateOnly(1995, 5, 5),
                Pwdhash = "",
                Pwdsalt = ""
            };
            context.Korisniks.Add(user);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var result = await controller.GetById(5);
            var returned = Assert.IsType<Korisnik>(result.Value);
            Assert.Equal("test", returned.Username);
        }

        [Fact]
        public async Task GetById_NotExists_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var result = await controller.GetById(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task RegistracijaVozac_Success_ReturnsId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());

            var dto = new RegistracijaVozacDTO
            {
                Username = "drv",
                Password = "pass",
                Ime = "D",
                Prezime = "R",
                Email = "drv@example.com",
                Telefon = "222",
                Datumrodjenja = new DateOnly(1995, 5, 5)
            };

            var json = JsonConvert.SerializeObject(dto);
            var result = await controller.RegistracijaVozac(json);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var id = Assert.IsType<int>(ok.Value);

            var saved = await context.Korisniks.FindAsync(id);
            Assert.Equal("drv", saved.Username);
            Assert.Equal("drv@example.com", saved.Email);
        }

        [Fact]
        public async Task RegistracijaVozac_DuplicateUsername_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var existing = new Korisnik
            {
                Idkorisnik = 1,
                Username = "drv",
                Ime = "X",
                Prezime = "Y",
                Email = "drv@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdhash = "",
                Pwdsalt = ""
            };
            context.Korisniks.Add(existing);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new RegistracijaVozacDTO
            {
                Username = "drv",
                Password = "p"
            };
            var json = JsonConvert.SerializeObject(dto);
            var result = await controller.RegistracijaVozac(json);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already exists", bad.Value.ToString());
        }

        [Fact]
        public async Task Registracija_Success_ReturnsId()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());

            var dto = new RegistracijaVozacDTO
            {
                Username = "u1",
                Password = "p1",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "333",
                Datumrodjenja = new DateOnly(1992, 2, 2),
                Vozacka = Convert.ToBase64String(new byte[] { 1 }),
                Osobna = Convert.ToBase64String(new byte[] { 2 }),
                Selfie = Convert.ToBase64String(new byte[] { 3 })
            };

            var result = await controller.Registracija(dto);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var id = Assert.IsType<int>(ok.Value);

            var saved = await context.Korisniks.FindAsync(id);
            Assert.Equal("u1", saved.Username);
            Assert.NotNull(saved.Imagevozacka);
        }

        [Fact]
        public async Task Registracija_DuplicateUsername_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var existing = new Korisnik
            {
                Idkorisnik = 1,
                Username = "u1",
                Ime = "X",
                Prezime = "Y",
                Email = "u1@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdhash = "",
                Pwdsalt = ""
            };
            context.Korisniks.Add(existing);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new RegistracijaVozacDTO
            {
                Username = "u1",
                Password = "p1"
            };
            var result = await controller.Registracija(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already exists", bad.Value.ToString());
        }

        [Fact]
        public async Task RegistracijaPutnik_Success_ReturnsDto()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());

            var dto = new RegistracijaPutnikDTO
            {
                Username = "p1",
                Password = "pp",
                Ime = "I",
                Prezime = "P",
                Email = "e@e",
                Telefon = "444",
                Datumrodjenja = new DateOnly(1993, 3, 3),
                Osobna = Convert.ToBase64String(new byte[] { 4 }),
                Selfie = Convert.ToBase64String(new byte[] { 5 })
            };
            var result = await controller.RegistracijaPutnik(dto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<RegistracijaPutnikDTO>(ok.Value);
            Assert.Equal("p1", returned.Username);
        }

        [Fact]
        public void Login_WrongUsername_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikLoginDTO { Username = "no", Password = "p" };
            var result = controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsUnauthorized()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());

            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash("right", salt);

            var user = new Korisnik
            {
                Idkorisnik = 2,
                Username = "user",
                Ime = "Test",
                Prezime = "User",
                Email = "user@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = salt,
                Pwdhash = hash
            };
            context.Korisniks.Add(user);
            context.SaveChanges();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikLoginDTO { Username = "user", Password = "wrong" };
            var result = controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task PotvrdiIliOdbijKorisnika_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var user = new Korisnik
            {
                Idkorisnik = 3,
                Ime = "X",
                Prezime = "Y",
                Email = "test@example.com",
                Telefon = "000",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Username = "toConfirm",
                Pwdsalt = "",
                Pwdhash = "",
                Isconfirmed = false
            };
            context.Korisniks.Add(user);
            await context.SaveChangesAsync();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new PotvrdaKorisnikDTO { Id = 3, IsConfirmed = true };
            var result = await controller.PotvrdiIliOdbijKorisnika(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("potvrđen", ok.Value.ToString());

            var updated = await context.Korisniks.FindAsync(3);
            Assert.True(updated.Isconfirmed.Value);
        }

        [Fact]
        public async Task PotvrdiIliOdbijKorisnika_NotFound_ReturnsNotFound()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new PotvrdaKorisnikDTO { Id = 999, IsConfirmed = false };
            var result = await controller.PotvrdiIliOdbijKorisnika(dto);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("nije pronađen", notFound.Value.ToString());
        }

        [Fact]
        public void ChangePassword_Success_ReturnsOk()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash("oldpass", salt);

            var user = new Korisnik
            {
                Idkorisnik = 4,
                Username = "cpuser",
                Ime = "Test",
                Prezime = "User",
                Email = "cpuser@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = salt,
                Pwdhash = hash
            };
            context.Korisniks.Add(user);
            context.SaveChanges();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikPromjenaLozinkeDTO
            {
                Username = "cpuser",
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };
            var result = controller.ChangePassword(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password was changed successfully", ok.Value);

            var updated = context.Korisniks.First(x => x.Idkorisnik == 4);
            Assert.NotEqual(hash, updated.Pwdhash);
        }

        [Fact]
        public void ChangePassword_UserNotExists_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikPromjenaLozinkeDTO
            {
                Username = "nouser",
                OldPassword = "x",
                NewPassword = "y"
            };
            var result = controller.ChangePassword(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("User does not exist", bad.Value.ToString());
        }

        [Fact]
        public void ChangePassword_WrongOldPassword_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash("correct", salt);

            var user = new Korisnik
            {
                Idkorisnik = 5,
                Username = "u",
                Ime = "Test",
                Prezime = "User",
                Email = "u@example.com",
                Telefon = "111",
                Datumrodjenja = new DateOnly(1990, 1, 1),
                Pwdsalt = salt,
                Pwdhash = hash
            };
            context.Korisniks.Add(user);
            context.SaveChanges();

            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikPromjenaLozinkeDTO
            {
                Username = "u",
                OldPassword = "wrong",
                NewPassword = "new"
            };
            var result = controller.ChangePassword(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Old password is incorrect", bad.Value.ToString());
        }

        [Fact]
        public void ChangePassword_InvalidInput_ReturnsBadRequest()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var controller = new KorisnikController(context, CreateConfiguration());
            var dto = new KorisnikPromjenaLozinkeDTO
            {
                Username = "",
                OldPassword = "",
                NewPassword = ""
            };
            var result = controller.ChangePassword(dto);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("There is no input", bad.Value.ToString());
        }
    }
}
