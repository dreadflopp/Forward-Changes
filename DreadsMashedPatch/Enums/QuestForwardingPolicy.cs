namespace DreadsMashedPatch.Enums;

/// <summary>
/// Controls whether the interdependent gameplay graph in QUEST records is merged field by field.
/// </summary>
public enum QuestForwardingPolicy
{
    /// <summary>
    /// A change to any structural QUEST property makes that complete override the new truth.
    /// </summary>
    AtomicOnStructuralChange,

    /// <summary>
    /// Forward every registered property independently.
    /// </summary>
    StandardForwarding
}
