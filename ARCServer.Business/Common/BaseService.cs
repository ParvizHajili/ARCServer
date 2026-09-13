using ARCServer.Business.Common.Validation;
using FluentValidation;

namespace ARCServer.Business.Common
{
    public abstract class BaseService
    {
        protected static async Task<ServiceResult<TResponse>?> ValidateAsync<TRequest, TResponse>(
            IValidator<TRequest> validator,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (validation.IsValid)
            {
                return null;
            }

            return ServiceResult<TResponse>.Failure(
                validation.ToErrorDictionary(),
                ServiceErrorType.Validation);
        }
    }
}
