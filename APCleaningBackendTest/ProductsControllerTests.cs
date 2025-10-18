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
    public class ProductsControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IBlobUploader> _mockBlobUploader;

        public ProductsControllerTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockBlobUploader = new Mock<IBlobUploader>();

            _mockConfig.Setup(c => c["Azure:StorageConnectionString"]).Returns("UseDevelopmentStorage=true");
            _mockConfig.Setup(c => c["Azure:ProductContainer"]).Returns("test-products");

            _mockBlobUploader.Setup(u => u.UploadAsync(It.IsAny<IFormFile>(), "test-products"))
                .ReturnsAsync("mocked-image.jpg");

            _mockBlobUploader.Setup(u => u.DeleteAsync(It.IsAny<string>(), "test-products"))
                .Returns(Task.CompletedTask);
        }

        private (ProductsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new ProductsController(context, _mockConfig.Object, _mockBlobUploader.Object);
            return (controller, context);
        }

        private IFormFile CreateFakeImage(string fileName = "test.jpg")
        {
            var content = "Fake image content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "ProductImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        [Fact]
        public async Task GetProducts_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetProductsDb");

            context.Product.Add(new Product
            {
                ProductName = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 10,
                Category = "Cleaning",
                IsAvailable = true,
                ProductImageUrl = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetProducts();
            var ok = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsAssignableFrom<List<Product>>(ok.Value);
            Assert.Single(products);
        }

        [Fact]
        public async Task GetProduct_ValidId_ReturnsProduct()
        {
            var (controller, context) = CreateControllerWithContext("GetProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 10,
                Category = "Cleaning",
                IsAvailable = true,
                ProductImageUrl = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetProduct(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(ok.Value);
            Assert.Equal("Test Product", product.ProductName);
        }

        [Fact]
        public async Task PostProduct_ValidModel_CreatesProduct()
        {
            var (controller, context) = CreateControllerWithContext("PostProductDb");

            var model = new ProductCreateModel
            {
                ProductName = "New Product",
                Description = "New Description",
                Price = 49.99m,
                StockQuantity = 5,
                Category = "Supplies",
                IsAvailable = true,
                ProductImage = CreateFakeImage()
            };

            var result = await controller.PostProduct(model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            var product = Assert.IsType<Product>(objectResult.Value);
            Assert.Equal("New Product", product.ProductName);
            Assert.Equal("mocked-image.jpg", product.ProductImageUrl);
        }

        [Fact]
        public async Task UpdateProduct_ValidId_UpdatesProduct()
        {
            var (controller, context) = CreateControllerWithContext("UpdateProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Old Product",
                Description = "Old Description",
                Price = 20.00m,
                StockQuantity = 3,
                Category = "Tools",
                IsAvailable = false,
                ProductImageUrl = "old.jpg"
            });

            await context.SaveChangesAsync();

            var model = new ProductCreateModel
            {
                ProductName = "Updated Product",
                Description = "Updated Description",
                Price = 25.00m,
                StockQuantity = 7,
                Category = "Tools",
                IsAvailable = true,
                ProductImage = CreateFakeImage("updated.jpg")
            };

            var result = await controller.UpdateProduct(1, model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal("Product updated successfully.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteProduct_ValidId_RemovesProduct()
        {
            var (controller, context) = CreateControllerWithContext("DeleteProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Delete Me",
                Description = "To be deleted",
                Price = 10.00m,
                StockQuantity = 1,
                Category = "Trash",
                IsAvailable = false,
                ProductImageUrl = "delete.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteProduct(1);
            Assert.IsType<NoContentResult>(result);
        }
    }
}

/* Code Attribution 
 * ------------------------------
 * Code by Copilot
 * Link: t
 * Accessed: 14 October 2025
 private (ProductsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new ProductsController(context, _mockConfig.Object, _mockBlobUploader.Object);
            return (controller, context);
        }

        private IFormFile CreateFakeImage(string fileName = "test.jpg")
        {
            var content = "Fake image content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "ProductImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        [Fact]
        public async Task GetProducts_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetProductsDb");

            context.Product.Add(new Product
            {
                ProductName = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 10,
                Category = "Cleaning",
                IsAvailable = true,
                ProductImageUrl = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetProducts();
            var ok = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsAssignableFrom<List<Product>>(ok.Value);
            Assert.Single(products);
        }

        [Fact]
        public async Task GetProduct_ValidId_ReturnsProduct()
        {
            var (controller, context) = CreateControllerWithContext("GetProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 10,
                Category = "Cleaning",
                IsAvailable = true,
                ProductImageUrl = "image.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetProduct(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(ok.Value);
            Assert.Equal("Test Product", product.ProductName);
        }

        [Fact]
        public async Task PostProduct_ValidModel_CreatesProduct()
        {
            var (controller, context) = CreateControllerWithContext("PostProductDb");

            var model = new ProductCreateModel
            {
                ProductName = "New Product",
                Description = "New Description",
                Price = 49.99m,
                StockQuantity = 5,
                Category = "Supplies",
                IsAvailable = true,
                ProductImage = CreateFakeImage()
            };

            var result = await controller.PostProduct(model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
            var product = Assert.IsType<Product>(objectResult.Value);
            Assert.Equal("New Product", product.ProductName);
            Assert.Equal("mocked-image.jpg", product.ProductImageUrl);
        }

        [Fact]
        public async Task UpdateProduct_ValidId_UpdatesProduct()
        {
            var (controller, context) = CreateControllerWithContext("UpdateProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Old Product",
                Description = "Old Description",
                Price = 20.00m,
                StockQuantity = 3,
                Category = "Tools",
                IsAvailable = false,
                ProductImageUrl = "old.jpg"
            });

            await context.SaveChangesAsync();

            var model = new ProductCreateModel
            {
                ProductName = "Updated Product",
                Description = "Updated Description",
                Price = 25.00m,
                StockQuantity = 7,
                Category = "Tools",
                IsAvailable = true,
                ProductImage = CreateFakeImage("updated.jpg")
            };

            var result = await controller.UpdateProduct(1, model);
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal("Product updated successfully.", objectResult.Value);
        }

        [Fact]
        public async Task DeleteProduct_ValidId_RemovesProduct()
        {
            var (controller, context) = CreateControllerWithContext("DeleteProductDb");

            context.Product.Add(new Product
            {
                ProductID = 1,
                ProductName = "Delete Me",
                Description = "To be deleted",
                Price = 10.00m,
                StockQuantity = 1,
                Category = "Trash",
                IsAvailable = false,
                ProductImageUrl = "delete.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteProduct(1);
            Assert.IsType<NoContentResult>(result);
        }
    }
 */