using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Marilog.Presentation.Extensions
{
    // Applies permissions to every controller action at startup.
    // 1. An action with its own authorization ([RequirePermission], [Authorize], [AllowAnonymous]) is left as is.
    // 2. A controller's entity comes from [PermissionEntity], else from its name, else "Laytime*" -> LaytimeCalculation.
    //    The HTTP verb and route then decide the action.
    // 3. Any other action in a controller with no authorization at all becomes super admin only (fail-closed).
    public sealed class PermissionConvention : IApplicationModelConvention
    {
        // POST routes that create new records. Any other POST must act on an existing record ("{id}/...").
        private static readonly HashSet<string> CreateRoutes =
            new(StringComparer.OrdinalIgnoreCase) { "", "batch", "bulk", "range", "import" };

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var controllerHasAuth = controller.Attributes.Any(a => a is IAuthorizeData or IAllowAnonymous);
                var entity = ResolveEntity(controller, controllerHasAuth);

                foreach (var action in controller.Actions)
                {
                    if (action.Attributes.Any(a => a is IAuthorizeData or IAllowAnonymous))
                        continue;

                    object metadata;
                    if (entity is not null)
                        metadata = new RequirePermissionAttribute(entity.Value, MapAction(controller, action));
                    else if (!controllerHasAuth)
                        metadata = new AuthorizeAttribute(AuthPolicies.SuperAdmin);
                    else
                        continue;

                    foreach (var selector in action.Selectors)
                        selector.EndpointMetadata.Add(metadata);
                }
            }
        }

        private static EntityType? ResolveEntity(ControllerModel controller, bool controllerHasAuth)
        {
            var entity = controller.Attributes.OfType<PermissionEntityAttribute>().FirstOrDefault()?.Entity;

            if (entity is null && !controllerHasAuth)
            {
                entity = ResolveFromName(controller.ControllerName);

                if (entity is null && controller.ControllerName.Contains("Laytime", StringComparison.OrdinalIgnoreCase))
                    entity = EntityType.LaytimeCalculation;
            }

            // Super-admin-only entities get no verb mapping: the whole controller stays super admin only.
            if (entity is not null && PermissionCatalog.SuperAdminOnlyEntities.Contains(entity.Value))
                return null;

            return entity;
        }

        private static PermissionAction MapAction(ControllerModel controller, ActionModel action)
        {
            var verbAttributes = action.Attributes.OfType<HttpMethodAttribute>().ToList();
            if (verbAttributes.Count != 1 || verbAttributes[0].HttpMethods.Count() != 1)
                throw Fail(controller, action, "must have exactly one HTTP verb attribute or an explicit [RequirePermission]");

            var verb = verbAttributes[0].HttpMethods.First().ToUpperInvariant();
            var template = (verbAttributes[0].Template ?? string.Empty).Trim('/');
            var actsOnExisting = template.Contains('{');

            return verb switch
            {
                "GET" or "HEAD" => PermissionAction.Read,
                "PUT" or "PATCH" => PermissionAction.Update,
                "POST" when CreateRoutes.Contains(template) => PermissionAction.Create,
                "POST" when actsOnExisting => PermissionAction.Update,
                "DELETE" when template.Length == 0 || IsSingleParameter(template) => PermissionAction.Delete,
                "DELETE" when actsOnExisting => PermissionAction.Update,
                _ => throw Fail(controller, action, $"'{verb} {template}' is ambiguous; add an explicit [RequirePermission]")
            };
        }

        // "{id:int}" -> true, "{id}/emails/{email}" -> false.
        private static bool IsSingleParameter(string template)
            => template.StartsWith('{') && template.EndsWith('}') && !template.Contains('/');

        // "Vessels" -> Vessel, "Companies" -> Company, "CharterTerms" -> CharterTerms.
        private static EntityType? ResolveFromName(string controllerName)
        {
            string[] candidates =
            [
                controllerName,
                controllerName.EndsWith("ies", StringComparison.OrdinalIgnoreCase) ? controllerName[..^3] + "y" : controllerName,
                controllerName.EndsWith('s') ? controllerName[..^1] : controllerName
            ];

            foreach (var name in candidates)
            {
                if (Enum.TryParse<EntityType>(name, ignoreCase: true, out var result)
                    && result != EntityType.NONE
                    && Enum.IsDefined(result))
                    return result;
            }

            return null;
        }

        private static InvalidOperationException Fail(ControllerModel controller, ActionModel action, string reason)
            => new($"{controller.ControllerName}.{action.ActionName}: {reason}.");
    }
}