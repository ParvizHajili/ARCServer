using ARCServer.Business.Common;
using ARCServer.Business.Dtos.Dashboard;

namespace ARCServer.Business.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<ServiceResult<DashboardOverviewDto>> GetOverviewAsync(
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> RecordViewByCodeAsync(
            string code,
            CancellationToken cancellationToken = default);
    }
}
