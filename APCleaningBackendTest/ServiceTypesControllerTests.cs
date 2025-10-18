using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class ServiceTypesControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IBlobUploader> _mockBlobUploader;

        public ServiceTypesControllerTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockBlobUploader = new Mock<IBlobUploader>();

            _mockConfig.Setup(c => c["Azure:ContainerName"]).Returns("serviceimages");

            _mockBlobUploader.Setup(u => u.UploadAsync(It.IsAny<IFormFile>(), "serviceimages"))
                .ReturnsAsync("mocked-service-image.jpg");

            _mockBlobUploader.Setup(u => u.DeleteAsync(It.IsAny<string>(), "serviceimages"))
                .Returns(Task.CompletedTask);
        }

        private (ServiceTypesController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new ServiceTypesController(context, _mockConfig.Object, _mockBlobUploader.Object);
            return (controller, context);
        }

        private IFormFile CreateFakeImage(string fileName = "test.jpg")
        {
            var content = "Fake image content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "ServiceImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        [Fact]
        public async Task GetServices_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetServicesDb");

            context.ServiceType.Add(new ServiceType
            {
                Name = "Test Service",
                Description = "Test Description",
                Price = 150,
                ImageURL = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetServices();
            var ok = Assert.IsType<ActionResult<IEnumerable<ServiceType>>>(result);
            var services = Assert.IsAssignableFrom<List<ServiceType>>(ok.Value);
            Assert.Single(services);
        }

        [Fact]
        public async Task GetService_ValidId_ReturnsService()
        {
            var (controller, context) = CreateControllerWithContext("GetServiceDb");

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Test Service",
                Description = "Test Description",
                Price = 150,
                ImageURL = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetService(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var service = Assert.IsType<ServiceType>(ok.Value);
            Assert.Equal("Test Service", service.Name);
        }

        [Fact]
        public async Task PostService_ValidModel_CreatesService()
        {
            var (controller, context) = CreateControllerWithContext("PostServiceDb");

            var model = new ServiceCreateModel
            {
                Name = "New Service",
                Description = "New Description",
                Price = 200,
                ServiceImage = CreateFakeImage()
            };

            var result = await controller.PostService(model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            var service = Assert.IsType<ServiceType>(objectResult.Value);
            Assert.Equal("New Service", service.Name);
            Assert.Equal("mocked-service-image.jpg", service.ImageURL);
        }

        [Fact]
        public async Task UpdateService_ValidId_UpdatesService()
        {
            var (controller, context) = CreateControllerWithContext("UpdateServiceDb");

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Old Service",
                Description = "Old Description",
                Price = 100,
                ImageURL = "old.jpg"
            });

            await context.SaveChangesAsync();

            var model = new ServiceUpdateModel
            {
                Name = "Updated Service",
                Description = "Updated Description",
                Price = 250,
                ServiceImage = CreateFakeImage("updated.jpg")
            };

            var result = await controller.UpdateService(1, model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal("Service updated successfully.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteService_ValidId_RemovesService()
        {
            var (controller, context) = CreateControllerWithContext("DeleteServiceDb");

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Delete Me",
                Description = "To be deleted",
                Price = 50,
                ImageURL = "delete.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteService(1);
            Assert.IsType<NoContentResult>(result);
        }
    }
}