using System.Windows;
using System.Windows.Input;
using DreadsMashedPatch.App.Services;

namespace DreadsMashedPatch.App;

public partial class InstallationPickerWindow : Window
{
    public InstallationPickerWindow(IReadOnlyList<PathDiscovery.Installation> installations)
    {
        InitializeComponent();
        InstallationList.ItemsSource = installations;
        InstallationList.SelectedIndex = installations.Count > 0 ? 0 : -1;
    }

    public PathDiscovery.Installation? SelectedInstallation =>
        InstallationList.SelectedItem as PathDiscovery.Installation;

    private void OnUseInstallation(object sender, RoutedEventArgs e)
    {
        if (SelectedInstallation is null)
        {
            return;
        }

        DialogResult = true;
    }

    private void OnInstallationDoubleClick(object sender, MouseButtonEventArgs e) => OnUseInstallation(sender, e);
}
