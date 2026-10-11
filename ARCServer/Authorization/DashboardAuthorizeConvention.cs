using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace ARCServer.Authorization
{
    /// <summary>
    /// Dashboard area controllers require JWT auth by default.
    /// Public (non-dashboard) endpoints stay anonymous — no per-action AllowAnonymous needed.
    /// </summary>
    public sealed class DashboardAuthorizeConvention : IControllerModelConvention
    {
        private static readonly AuthorizationPolicy DashboardPolicy =
            new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

        public void Apply(ControllerModel controller)
        {
            if (!IsDashboardController(controller))
            {
                return;
            }

            // AuthController: login must stay public; [Authorize] is applied per-action (e.g. me).
            if (controller.ControllerName.Equals("Auth", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (HasAuthorizeMetadata(controller))
            {
                return;
            }

            controller.Filters.Add(new AuthorizeFilter(DashboardPolicy));
        }

        private static bool IsDashboardController(ControllerModel controller)
        {
            if (controller.Attributes.OfType<AreaAttribute>()
                .Any(a => a.RouteValue.Equals("Dashboard", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var ns = controller.ControllerType.Namespace;
            return ns != null
                && ns.Contains(".Areas.Dashboard", StringComparison.Ordinal);
        }

        private static bool HasAuthorizeMetadata(ControllerModel controller)
        {
            return controller.Filters.OfType<AuthorizeFilter>().Any()
                || controller.Attributes.OfType<IAuthorizeData>().Any();
        }
    }
}
