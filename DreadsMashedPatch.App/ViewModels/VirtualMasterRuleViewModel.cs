namespace ForwardChanges.App.ViewModels;

public sealed class VirtualMasterRuleViewModel : BindableBase
{
    private string _injectedMaster = string.Empty;
    private string _targetModsText = string.Empty;

    public VirtualMasterRuleViewModel(VirtualMasterRule rule)
    {
        _injectedMaster = rule.InjectedMaster;
        _targetModsText = string.Join(
            Environment.NewLine,
            rule.TargetMods.Order(StringComparer.OrdinalIgnoreCase));
    }

    public string InjectedMaster
    {
        get => _injectedMaster;
        set
        {
            if (SetProperty(ref _injectedMaster, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string TargetModsText
    {
        get => _targetModsText;
        set => SetProperty(ref _targetModsText, value ?? string.Empty);
    }

    public string DisplayName => string.IsNullOrWhiteSpace(InjectedMaster)
        ? "New compatibility rule"
        : InjectedMaster.Trim();

    public VirtualMasterRule ToModel() => new()
    {
        InjectedMaster = InjectedMaster,
        TargetMods = TargetModsText
            .Split(['\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
    };
}
