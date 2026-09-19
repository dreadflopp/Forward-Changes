namespace DreadsMashedPatch.Enums;

/// <summary>
/// Controls whether coupled PERK gameplay fields are merged independently or
/// establish a complete-record ownership boundary.
/// </summary>
public enum PerkForwardingPolicy
{
    AtomicOnCoupledPropertyChange,
    StandardForwarding
}
