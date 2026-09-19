namespace DreadsMashedPatch.Enums;

/// <summary>
/// Controls whether Story Manager behavior graphs are combined field by field.
/// </summary>
public enum StoryManagerForwardingPolicy
{
    /// <summary>
    /// Topology, condition, flag, event, and limit changes establish a complete-node
    /// ownership boundary. Quest rows may still merge while that configuration is stable.
    /// </summary>
    AtomicOnConfigurationChange,

    /// <summary>
    /// Forward every registered property independently.
    /// </summary>
    StandardForwarding
}
