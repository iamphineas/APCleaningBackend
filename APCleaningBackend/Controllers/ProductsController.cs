using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IConfiguration _config;

        public ProductsController(APCleaningBackendContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Product.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductCreateModel model)
        {
            string uploadedFileName = null;

            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                try
                {
                    var connectionString = _config["Azure:StorageConnectionString"];
                    var containerName = _config["Azure:ProductContainer"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    uploadedFileName = $"{Guid.NewGuid()}_{model.ProductImage.FileName}";
                    var blobClient = containerClient.GetBlobClient(uploadedFileName);

                    using var stream = model.ProductImage.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Blob upload failed: {ex.Message}");
                    return StatusCode(500, "File upload failed.");
                }
            }

            var product = new Product
            {
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                Category = model.Category,
                IsAvailable = model.IsAvailable,
                ProductImageUrl = uploadedFileName
            };

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductCreateModel model)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            string uploadedFileName = product.ProductImageUrl;

            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                try
                {
                    var connectionString = _config["Azure:StorageConnectionString"];
                    var containerName = _config["Azure:ProductContainer"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    uploadedFileName = $"{Guid.NewGuid()}_{model.ProductImage.FileName}";
                    var blobClient = containerClient.GetBlobClient(uploadedFileName);

                    using var stream = model.ProductImage.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Blob upload failed: {ex.Message}");
                    return StatusCode(500, "File upload failed.");
                }
            }

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Category = model.Category;
            product.IsAvailable = model.IsAvailable;
            product.ProductImageUrl = uploadedFileName;

            _context.Product.Update(product);
            await _context.SaveChangesAsync();

            return Ok("Product updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            if (!string.IsNullOrEmpty(product.ProductImageUrl))
            {
                try
                {
                    var connectionString = _config["Azure:StorageConnectionString"];
                    var containerName = _config["Azure:ProductContainer"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    var blobClient = containerClient.GetBlobClient(product.ProductImageUrl);

                    await blobClient.DeleteIfExistsAsync();
                    Console.WriteLine($"Image deleted from Azure: {product.ProductImageUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image deletion failed: {ex.Message}");
                }
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Product deleted: {product.ProductName} (ID: {product.ProductID})");
            return NoContent();
        }
    }
}
