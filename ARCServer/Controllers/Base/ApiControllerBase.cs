using ARCServer.Business.Common;
using Microsoft.AspNetCore.Mvc;

namespace ARCServer.Controllers.Base
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult FromResult<T>(ServiceResult<T> result)
        {
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }

            return FromFailure(result);
        }

        protected IActionResult FromCreatedResult<T>(ServiceResult<T> result, string actionName, object routeValues)
        {
            if (result.Succeeded)
            {
                return CreatedAtAction(actionName, routeValues, result.Data);
            }

            return FromFailure(result);
        }

        protected IActionResult FromFailure<T>(ServiceResult<T> result)
        {
            if (result.IsNotFound)
            {
                return NotFound(new
                {
                    title = "Resurs tapılmadı",
                    errors = result.Errors,
                });
            }

            foreach (var (key, messages) in result.Errors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(key, message);
                }
            }

            return ValidationProblem(ModelState);
        }
    }
}
