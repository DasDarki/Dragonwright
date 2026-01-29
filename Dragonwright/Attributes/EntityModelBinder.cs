using System.Reflection;
using Dragonwright.Database;
using Dragonwright.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dragonwright.Attributes;

/// <summary>
/// Model binder that resolves an entity from the database by its ID.
/// Stores error information in <see cref="HttpContext.Items"/> so that
/// <see cref="EntityValidationFilter"/> can short-circuit with the appropriate status code.
/// </summary>
internal sealed class EntityModelBinder : IModelBinder
{
    internal const string ErrorKey = "__fromEntity_error";

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var actionDescriptor = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
        var paramInfo = actionDescriptor?.MethodInfo.GetParameters()
            .FirstOrDefault(p => p.Name == bindingContext.ModelMetadata.ParameterName);
        var attribute = paramInfo?.GetCustomAttribute<FromEntityAttribute>();

        if (attribute == null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var rawId = attribute.Source switch
        {
            EntitySource.Path => bindingContext.HttpContext.Request.RouteValues
                .TryGetValue(attribute.IdName, out var routeVal)
                ? routeVal?.ToString()
                : null,

            EntitySource.Query => bindingContext.HttpContext.Request.Query
                .TryGetValue(attribute.IdName, out var queryVal)
                ? queryVal.ToString()
                : null,

            EntitySource.Header => bindingContext.HttpContext.Request.Headers
                .TryGetValue(attribute.IdName, out var headerVal)
                ? headerVal.ToString()
                : null,

            _ => null
        };

        if (string.IsNullOrEmpty(rawId) || !Guid.TryParse(rawId, out var id))
        {
            bindingContext.HttpContext.Items[ErrorKey] = new EntityBindingError(
                EntityBindingErrorKind.InvalidId, attribute.IdName);
            bindingContext.Result = ModelBindingResult.Success(null!);
            return;
        }

        var databaseService = bindingContext.HttpContext.RequestServices.GetRequiredService<DatabaseService>();
        await using var dbContext = databaseService.CreateDbContext();
        var entity = await dbContext.FindAsync(bindingContext.ModelType, id);

        if (entity == null)
        {
            bindingContext.HttpContext.Items[ErrorKey] = new EntityBindingError(
                EntityBindingErrorKind.NotFound, attribute.IdName);
            bindingContext.Result = ModelBindingResult.Success(null!);
            return;
        }

        bindingContext.Result = ModelBindingResult.Success(entity);
    }
}

internal enum EntityBindingErrorKind
{
    InvalidId,
    NotFound
}

internal sealed record EntityBindingError(EntityBindingErrorKind Kind, string IdName);

/// <summary>
/// Action filter that checks for entity binding errors stored by <see cref="EntityModelBinder"/>
/// and short-circuits the pipeline with the appropriate HTTP status code.
/// Runs before <c>ModelStateInvalidFilter</c> (order -2000) to prevent spurious 400 responses.
/// </summary>
internal sealed class EntityValidationFilter : IActionFilter, IOrderedFilter
{
    /// <summary>
    /// Runs before the built-in <c>ModelStateInvalidFilter</c> (order -2000).
    /// </summary>
    public int Order => -2001;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Items.TryGetValue(EntityModelBinder.ErrorKey, out var value)
            || value is not EntityBindingError error)
            return;

        context.Result = error.Kind switch
        {
            EntityBindingErrorKind.NotFound => new NotFoundResult(),
            EntityBindingErrorKind.InvalidId => new BadRequestObjectResult(new
            {
                message = $"Missing or invalid entity ID '{error.IdName}'."
            }),
            _ => new BadRequestResult()
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
