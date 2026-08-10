using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.ViewModels;

public partial class ServiceInfoViewModel : ObservableObject
{
    private static readonly Uri WhatsAppUri = new("https://wa.me/919867343302");

    public ObservableCollection<ServiceInfoCategory> Categories { get; } = new(ServiceInfoCategories.All);

    [ObservableProperty] private ServiceInfoCategory selectedCategory;

    public ServiceInfoViewModel()
    {
        selectedCategory = Categories[0];
    }

    [RelayCommand]
    private async Task ChatOnWhatsAppAsync() => await Launcher.Default.OpenAsync(WhatsAppUri);

    [RelayCommand]
    private async Task SchedulePickupAsync() => await Shell.Current.GoToAsync("//ShopPage");
}
