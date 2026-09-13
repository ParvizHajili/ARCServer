using Microsoft.AspNetCore.Http;

namespace ARCServer.Business.Services.Storage
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Şəkili Cloudinary-ə yükləyir. <paramref name="folder"/> dinamik qovluq adıdır
        /// (məs: <c>CloudinaryFolders.Categories</c>).
        /// </summary>
        /// <returns>Secure URL</returns>
        string Upload(IFormFile file, string folder);

        Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    }
}
