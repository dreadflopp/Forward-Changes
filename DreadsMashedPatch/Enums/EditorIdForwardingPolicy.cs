namespace DreadsMashedPatch.Enums;

public enum EditorIdForwardingPolicy
{
    /// <summary>
    /// Keep the Editor ID from the latest official baseline override.
    /// </summary>
    PreserveBaseline,

    /// <summary>
    /// Treat Editor IDs like other independently forwarded properties.
    /// </summary>
    StandardForwarding,

    /// <summary>
    /// Forward a computed Editor ID only when another property also requires an override.
    /// </summary>
    ForwardOnlyWithOtherChanges
}
