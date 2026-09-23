namespace DreadsMashedPatch.RecordHandlers;

public sealed class PropertyForwardingCoordination
{
    public static PropertyForwardingCoordination None { get; } = new(
        new Dictionary<string, object?>(),
        null);

    public PropertyForwardingCoordination(
        IReadOnlyDictionary<string, object?> forwardValues,
        string? auditMessage)
    {
        ForwardValues = forwardValues;
        AuditMessage = auditMessage;
    }

    public IReadOnlyDictionary<string, object?> ForwardValues { get; }

    public string? AuditMessage { get; }
}
