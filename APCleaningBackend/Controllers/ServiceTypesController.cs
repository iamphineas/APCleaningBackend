using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypesController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IBlobUploader _blobUploader;
        private readonly string _serviceContainer;

        public ServiceTypesController(APCleaningBackendContext context, IConfiguration config, IBlobUploader blobUploader)
        {
            _context = context;
            _blobUploader = blobUploader;
            _serviceContainer = config["Azure:ContainerName"];
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceType>>> GetServices()
        {
            return await _context.ServiceType.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceType>> GetService(int id)
        {
            var service = await _context.ServiceType.FindAsync(id);
            return service == null ? NotFound() : Ok(service);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceType>> PostService([FromForm] ServiceCreateModel model)
        {
            string uploadedFileName = null;

            try
            {
                uploadedFileName = await _blobUploader.UploadAsync(model.ServiceImage, _serviceContainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blob upload failed: {ex.Message}");
                return StatusCode(500, "File upload failed.");
            }

            var service = new ServiceType
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                ImageURL = uploadedFileName
            };

            _context.ServiceType.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceTypeID }, service);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromForm] ServiceUpdateModel model)
        {
            var service = await _context.ServiceType.FindAsync(id);
            if (service == null)
                return NotFound("Service record not found.");

            string uploadedFileName = service.ImageURL;

            if (model.ServiceImage != null && model.ServiceImage.Length > 0)
            {
                try
                {
                    uploadedFileName = await _blobUploader.UploadAsync(model.ServiceImage, _serviceContainer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Blob upload failed: {ex.Message}");
                    return StatusCode(500, "File upload failed.");
                }
            }

            service.Name = model.Name;
            service.Description = model.Description;
            service.Price = model.Price;
            service.ImageURL = uploadedFileName;

            _context.ServiceType.Update(service);
            await _context.SaveChangesAsync();

            return Ok("Service updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.ServiceType.FindAsync(id);
            if (service == null)
                return NotFound("Service not found.");

            if (!string.IsNullOrEmpty(service.ImageURL))
            {
                try
                {
                    await _blobUploader.DeleteAsync(service.ImageURL, _serviceContainer);
                    Console.WriteLine($"Image deleted from Azure: {service.ImageURL}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Image deletion failed: {ex.Message}");
                }
            }

            _context.ServiceType.Remove(service);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Service deleted: {service.Name} (ID: {service.ServiceTypeID})");
            return NoContent();
        }
    }
}