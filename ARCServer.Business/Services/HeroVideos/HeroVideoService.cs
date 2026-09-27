using ARCServer.Business.Common;
using ARCServer.Business.Common.Storage;
using ARCServer.Business.Services.Storage;
using ARCServer.Data.Repositories;
using ARCServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ARCServer.Business.Services.HeroVideos
{
    public class HeroVideoDto
    {
        public string? VideoUrl { get; set; }
    }

    public interface IHeroVideoService
    {
        Task<ServiceResult<HeroVideoDto>> GetAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<HeroVideoDto>> SaveAsync(
            Microsoft.AspNetCore.Http.IFormFile video,
            int? userId = null,
            CancellationToken cancellationToken = default);
    }

    public class HeroVideoService : IHeroVideoService
    {
        private readonly IRepository<HeroVideo> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public HeroVideoService(
            IRepository<HeroVideo> repository,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ServiceResult<HeroVideoDto>> GetAsync(
            CancellationToken cancellationToken = default)
        {
            var current = await CurrentAsync(cancellationToken);
            return ServiceResult<HeroVideoDto>.Success(new HeroVideoDto
            {
                VideoUrl = current?.VideoUrl,
            });
        }

        public async Task<ServiceResult<HeroVideoDto>> SaveAsync(
            Microsoft.AspNetCore.Http.IFormFile video,
            int? userId = null,
            CancellationToken cancellationToken = default)
        {
            string url;
            try
            {
                url = await _cloudinaryService.UploadVideoAsync(
                    video,
                    CloudinaryFolders.HeroVideo,
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return ServiceResult<HeroVideoDto>.Failure("video", ex.Message);
            }
            catch (Exception)
            {
                return ServiceResult<HeroVideoDto>.Failure(
                    "video",
                    "Video yüklənərkən xəta baş verdi.");
            }

            var now = DateTime.UtcNow;
            var existing = await _repository.Query()
                .Where(x => x.Deleted == 0)
                .ToListAsync(cancellationToken);

            foreach (var item in existing)
            {
                _repository.SoftDelete(item, userId);
            }

            await _repository.AddAsync(new HeroVideo
            {
                VideoUrl = url,
                CreateDate = now,
                CreatorId = userId,
                Deleted = 0,
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<HeroVideoDto>.Success(new HeroVideoDto { VideoUrl = url });
        }

        private Task<HeroVideo?> CurrentAsync(CancellationToken cancellationToken)
        {
            return _repository.Query()
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
