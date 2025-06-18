/// <summary>
/// Represents an item in a property context with its owning mod.
/// </summary>
/// <typeparam name="T">The type of the item</typeparam>
/// <remarks>
/// - Item may be null to represent unset properties in Skyrim
/// - OwnerMod will always be set to the mod that last modified the item
/// </remarks>
namespace ForwardChanges.Contexts.Interfaces
{
    public interface IItemContext<T>
    {
        /// <summary>
        /// The item value. May be null to represent unset properties in Skyrim.
        /// </summary>
        T? Item { get; set; }

        /// <summary>
        /// The mod that last modified this item. Will always be set.
        /// </summary>
        string OwnerMod { get; set; }
    }
}