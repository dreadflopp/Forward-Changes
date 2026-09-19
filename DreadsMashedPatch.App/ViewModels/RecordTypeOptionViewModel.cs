namespace DreadsMashedPatch.App.ViewModels;

public sealed class RecordTypeOptionViewModel : BindableBase
{
    private bool _isEnabled;

    public RecordTypeOptionViewModel(
        string signature,
        string displayName,
        IReadOnlyList<Type> recordTypes,
        bool isEnabled)
    {
        Signature = signature;
        DisplayName = displayName;
        RecordTypes = recordTypes;
        _isEnabled = isEnabled;
    }

    public string Signature { get; }

    public IReadOnlyList<Type> RecordTypes { get; }

    public IEnumerable<string> Ids => RecordTypes.Select(x => x.FullName ?? x.Name);

    public string DisplayName { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}
