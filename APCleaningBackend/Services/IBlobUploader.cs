using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace APCleaningBackend.Services
{
    public interface IBlobUploader
    {
        Task<string> UploadAsync(IFormFile file, string containerName);
        Task DeleteAsync(string fileName, string containerName);
    }
}