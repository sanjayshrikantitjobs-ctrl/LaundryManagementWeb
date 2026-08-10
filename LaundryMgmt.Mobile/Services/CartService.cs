using System.Collections.ObjectModel;
using LaundryMgmt.Mobile.Models;

namespace LaundryMgmt.Mobile.Services;

/// <summary>In-memory cart for the current app session — not persisted to Preferences
/// across an app kill/restart (a deliberate simplification vs. web's sessionStorage
/// version; there's no native mobile equivalent worth the complexity here).
/// Scoped per logged-in user id via SetCurrentUser, mirroring the fix client-web
/// needed for "cart leaking across customer logins" — AuthService calls this on
/// every login/logout so switching accounts always starts with an empty cart.</summary>
public class CartService
{
    public ObservableCollection<CartItem> Items { get; } = new();

    public int ItemCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);

    private string? _currentUserId;

    public void SetCurrentUser(string? userId)
    {
        if (userId == _currentUserId) return;
        _currentUserId = userId;
        Items.Clear();
    }

    public void Add(CartItem item)
    {
        var index = IndexOf(item.GarmentId, item.ServiceId);
        if (index >= 0)
        {
            var existing = Items[index];
            Items[index] = existing with { Quantity = existing.Quantity + item.Quantity, WeightKg = item.WeightKg ?? existing.WeightKg };
        }
        else
        {
            Items.Add(item);
        }
    }

    public void UpdateQuantity(Guid garmentId, Guid serviceId, int quantity)
    {
        if (quantity <= 0)
        {
            Remove(garmentId, serviceId);
            return;
        }

        var index = IndexOf(garmentId, serviceId);
        if (index >= 0)
            Items[index] = Items[index] with { Quantity = quantity };
    }

    public void Remove(Guid garmentId, Guid serviceId)
    {
        var item = Items.FirstOrDefault(i => i.GarmentId == garmentId && i.ServiceId == serviceId);
        if (item is not null)
            Items.Remove(item);
    }

    public void Clear() => Items.Clear();

    private int IndexOf(Guid garmentId, Guid serviceId)
    {
        for (var i = 0; i < Items.Count; i++)
            if (Items[i].GarmentId == garmentId && Items[i].ServiceId == serviceId)
                return i;
        return -1;
    }
}
