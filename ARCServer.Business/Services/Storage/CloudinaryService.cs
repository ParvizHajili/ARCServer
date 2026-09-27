using ARCServer.Business.Common.Messages;
using ARCServer.Business.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ARCServer.Business.Services.Storage
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinarySettings _settings;
        private readonly Lazy<Cloudinary> _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            _settings = options.Value;
            _cloudinary = new Lazy<Cloudinary>(CreateClient);
        }

        public string Upload(IFormFile file, string folder)
        {
            ValidateFile(file, folder);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder.Trim().Trim('/'),
            };

            var uploadResult = _cloudinary.Value.Upload(uploadParams);

            if (uploadResult.Error is not null)
            {
                throw new InvalidOperationException(
                    $"Cloudinary yükləmə xətası: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl?.ToString()
                ?? throw new InvalidOperationException("Cloudinary secure URL qaytarmadı.");
        }

        public Task<string> UploadAsync(
            IFormFile file,
            string folder,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Upload(file, folder));
        }

        public Task<string> UploadVideoAsync(
            IFormFile file,
            string folder,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateVideo(file, folder);

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder.Trim().Trim('/'),
            };

            var uploadResult = _cloudinary.Value.Upload(uploadParams);
            if (uploadResult.Error is not null)
            {
                throw new InvalidOperationException(
                    $"Cloudinary yükləmə xətası: {uploadResult.Error.Message}");
            }

            return Task.FromResult(uploadResult.SecureUrl?.ToString()
                ?? throw new InvalidOperationException("Cloudinary secure URL qaytarmadı."));
        }

        private Cloudinary CreateClient()
        {
            if (string.IsNullOrWhiteSpace(_settings.CloudName)
                || string.IsNullOrWhiteSpace(_settings.ApiKey)
                || string.IsNullOrWhiteSpace(_settings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "CloudinarySettings (CloudName, ApiKey, ApiSecret) appsettings-də doldurulmalıdır.");
            }

            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            return new Cloudinary(account);
        }

        private static void ValidateFile(IFormFile file, string folder)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException(
                    ErrorMessages.Format(ErrorMessages.Common.NotEmpty, ErrorMessages.Fields.Image),
                    nameof(file));
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("Cloudinary folder adı boş ola bilməz.", nameof(folder));
            }
        }

        private static void ValidateVideo(IFormFile file, string folder)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException("Video boş ola bilməz.", nameof(file));
            }

            if (file.Length > 25 * 1024 * 1024)
            {
                throw new ArgumentException("Video 25 MB-dan böyük ola bilməz.", nameof(file));
            }

            var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
            if (!contentType.StartsWith("video/"))
            {
                throw new ArgumentException("Yalnız video fayl yüklənə bilər.", nameof(file));
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("Cloudinary folder adı boş ola bilməz.", nameof(folder));
            }
        }
    }
}
