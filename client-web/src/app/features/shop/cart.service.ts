import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { PricingType } from '../../core/models/catalog.models';
import { AuthService } from '../../core/services/auth.service';

export interface CartItemAddOn {
  id: string;
  name: string;
  price: number;
}

export interface CartItem {
  garmentId: string;
  garmentName: string;
  garmentImageUrl?: string;
  categoryName: string;
  serviceId: string;
  serviceName: string;
  pricingType: PricingType;
  unitPrice: number;
  quantity: number;
  weightKg?: number;
  expressSurcharge: number;
  selectedAddOns?: CartItemAddOn[];
}

const CART_KEY_PREFIX = 'laundry_mgmt_cart_';

/// <summary>Cart contents are scoped per logged-in user id, not a single shared
/// sessionStorage key — otherwise one customer's selections would still be sitting
/// in the cart after another customer logs into the same browser tab.</summary>
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly auth = inject(AuthService);
  private readonly _items = signal<CartItem[]>([]);
  readonly items = this._items.asReadonly();

  private currentUserId: string | null = null;

  readonly itemCount = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));

  readonly subtotal = computed(() =>
    this._items().reduce((sum, i) => sum + this.lineTotal(i), 0)
  );

  constructor() {
    effect(() => {
      const userId = this.auth.currentUser()?.userId ?? null;
      if (userId !== this.currentUserId) {
        this.currentUserId = userId;
        this._items.set(userId ? this.restore(userId) : []);
      }
    });

    effect(() => {
      const items = this._items();
      if (this.currentUserId) {
        sessionStorage.setItem(this.storageKey(this.currentUserId), JSON.stringify(items));
      }
    });
  }

  lineTotal(item: CartItem): number {
    const base = item.pricingType === PricingType.WeightBased ? item.unitPrice * (item.weightKg || 0) : item.unitPrice * item.quantity;
    const addOnsTotal = (item.selectedAddOns ?? []).reduce((sum, a) => sum + a.price, 0);
    return base + addOnsTotal;
  }

  /// <summary>Includes the selected add-on ids so that the same garment+service with
  /// a different add-on combination becomes a distinct line rather than silently
  /// merging quantities (e.g. "Shirt+Wash" and "Shirt+Wash+Stain Removal" must stay
  /// separate).</summary>
  key(garmentId: string, serviceId: string, addOnIds: string[] = []): string {
    const addOnsKey = [...addOnIds].sort().join(',');
    return `${garmentId}:${serviceId}:${addOnsKey}`;
  }

  private keyOf(item: CartItem): string {
    return this.key(item.garmentId, item.serviceId, (item.selectedAddOns ?? []).map((a) => a.id));
  }

  add(item: CartItem): void {
    const key = this.keyOf(item);
    const existing = this._items().find((i) => this.keyOf(i) === key);

    if (existing) {
      this._items.update((items) =>
        items.map((i) =>
          this.keyOf(i) === key
            ? { ...i, quantity: i.quantity + item.quantity, weightKg: item.weightKg ?? i.weightKg }
            : i
        )
      );
    } else {
      this._items.update((items) => [...items, item]);
    }
  }

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity <= 0) {
      this.remove(item);
      return;
    }
    const key = this.keyOf(item);
    this._items.update((items) =>
      items.map((i) => (this.keyOf(i) === key ? { ...i, quantity } : i))
    );
  }

  remove(item: CartItem): void {
    const key = this.keyOf(item);
    this._items.update((items) => items.filter((i) => this.keyOf(i) !== key));
  }

  clear(): void {
    this._items.set([]);
  }

  private storageKey(userId: string): string {
    return `${CART_KEY_PREFIX}${userId}`;
  }

  private restore(userId: string): CartItem[] {
    const raw = sessionStorage.getItem(this.storageKey(userId));
    return raw ? (JSON.parse(raw) as CartItem[]) : [];
  }
}
