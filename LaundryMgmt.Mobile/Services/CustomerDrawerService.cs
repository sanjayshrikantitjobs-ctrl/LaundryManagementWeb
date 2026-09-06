using CommunityToolkit.Mvvm.ComponentModel;

namespace LaundryMgmt.Mobile.Services;

/// <summary>Shared open/closed state for the customer app's left navigation drawer.
/// Singleton so the single hamburger button living in AppShell's global title bar can
/// toggle it, while each customer page hosts its own <see cref="Views.CustomerDrawer"/>
/// overlay that reacts to the same state — no per-page plumbing through a ViewModel
/// required.</summary>
public partial class CustomerDrawerService : ObservableObject
{
    [ObservableProperty] private bool isOpen;

    public void Toggle() => IsOpen = !IsOpen;

    public void Close() => IsOpen = false;
}
