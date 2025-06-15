namespace ForwardChanges.PropertyStates
{
    /// <summary>
    /// Represents the state of a property.
    /// </summary>
    /// <remarks>
    /// Contains metadata about the property's state in the mod load order:
    /// <list type="bullet">
    ///     <item><description><see cref="IsResolved"/>: Indicates if the property has been resolved</description></item>
    ///     <item><description><see cref="ShouldForward"/>: A boolean indicating if the property should be forwarded to the patcher. Not tracked by this state</description></item>
    ///     <item><description><see cref="OriginalValue"/>: The original value of the property</description></item>
    ///     <item><description><see cref="FinalValue"/>: The final value of the property</description></item>
    ///     <item><description><see cref="LastChangedByMod"/>: The mod that last modified the property</description></item>
    /// </list>
    /// </remarks>
    public class PropertyState
    {
        public bool IsResolved { get; set; } = false;
        public bool ShouldForward { get; set; } = false;
        public object? OriginalValue { get; set; } = null;
        public object? FinalValue { get; set; } = null;
        public string LastChangedByMod { get; set; } = string.Empty;
    }
}