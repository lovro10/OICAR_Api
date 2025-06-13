using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using REST_API___oicar.Controllers;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace APIUnitTests
{
    public class CarBrandControllerTests : IDisposable
    {
        private readonly string _filesDir;
        private readonly string _filePath;

        public CarBrandControllerTests()
        {
            _filesDir = Path.Combine(Directory.GetCurrentDirectory(), "Files");
            _filePath = Path.Combine(_filesDir, "Cars.txt");
            if (Directory.Exists(_filesDir))
                Directory.Delete(_filesDir, true);
        }

        public void Dispose()
        {
            if (Directory.Exists(_filesDir))
                Directory.Delete(_filesDir, true);
        }

        [Fact]
        public async Task GetAllCarBrands_FileNotFound_ReturnsNotFound()
        {
            var controller = new CarBrandController();
            var result = await controller.GetAllCarBrands();
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("File not found.", notFound.Value);
        }

        [Fact]
        public async Task GetAllCarBrands_EmptyFile_ReturnsEmptyList()
        {
            Directory.CreateDirectory(_filesDir);
            File.WriteAllLines(_filePath, Array.Empty<string>());
            var controller = new CarBrandController();
            var result = await controller.GetAllCarBrands();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<string>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetAllCarBrands_ValidAndBlankLines_ReturnsTrimmedNonEmpty()
        {
            Directory.CreateDirectory(_filesDir);
            File.WriteAllLines(_filePath, new[]
            {
                " Toyota ",
                "Honda",
                "",
                "   ",
                "BMW  "
            });
            var controller = new CarBrandController();
            var result = await controller.GetAllCarBrands();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<string>>(ok.Value);
            Assert.Equal(new List<string> { "Toyota", "Honda", "BMW" }, list);
        }
    }
}
