/// <summary>
/// Represents a property value with its owning mod and other contexts
/// </summary>
/// <typeparam name="T">The type of the property value</typeparam>
/// <remarks>
/// - Value may be null to represent unset properties in Skyrim
/// - OwnerMod will always be set to the mod that last modified the property
/// </remarks>
namespace ForwardChanges.Contexts.Interfaces
{
    public interface IPropertyValueContext<T>
    {
        /// <summary>
        /// The mod that last modified this property. Will always be set.
        /// </summary>
        string OwnerMod { get; set; }
    }
}