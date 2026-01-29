using Microsoft.AspNetCore.Mvc;

namespace Dragonwright.Attributes;

/// <summary>
/// Specifies where the entity ID should be resolved from.
/// </summary>
public enum EntitySource
{
    /// <summary>
    /// Resolve the ID from the route/path parameters (default).
    /// </summary>
    Path,

    /// <summary>
    /// Resolve the ID from query string parameters.
    /// </summary>
    Query,

    /// <summary>
    /// Resolve the ID from a request header.
    /// </summary>
    Header
}

/// <summary>
/// Binds an action parameter to an entity loaded from the database by its ID.
/// The ID is resolved from the request based on the specified <see cref="Source"/>.
/// Returns 404 if the entity is not found, or 400 if the ID is missing/invalid.
/// </summary>
/// <example>
/// <code>
/// [HttpGet("{id:guid}")]
/// public IActionResult GetUser([FromEntity] User user) { ... }
///
/// [HttpGet("{userId:guid}")]
/// public IActionResult GetUser([FromEntity("userId")] User user) { ... }
///
/// [HttpGet("")]
/// public IActionResult GetUser([FromEntity("userId", EntitySource.Query)] User user) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromEntityAttribute : ModelBinderAttribute
{
    /// <summary>
    /// The name of the parameter containing the entity ID.
    /// </summary>
    public string IdName { get; }

    /// <summary>
    /// The source from which to resolve the entity ID.
    /// </summary>
    public EntitySource Source { get; }

    public FromEntityAttribute(string idName = "id", EntitySource source = EntitySource.Path)
    {
        IdName = idName;
        Source = source;
        BinderType = typeof(EntityModelBinder);
    }
}
