import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { PricingType } from '../../core/models/catalog.models';
import { AuthService } from '../../core/services/auth.service';

export interface CartItem {
  garmentId: string;
  garmentName: string;
  garmentImageUrl?: string;
  garmentCategory: string;
  serviceId: string;
  serviceName: string;
  pricingType: PricingType;
  unitPrice: number;
  quantity: number;
  weightKg?: number;
  expressSurcharge: number;
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
    return item.pricingType === PricingType.WeightBased ? item.unitPrice * (item.weightKg || 0) : item.unitPrice * item.quantity;
  }

  key(garmentId: string, serviceId: string): string {
    return `${garmentId}:${serviceId}`;
  }

  add(item: CartItem): void {
    const key = this.key(item.garmentId, item.serviceId);
    const existing = this._items().find((i) => this.key(i.garmentId, i.serviceId) === key);

    if (existing) {
      this._items.update((items) =>
        items.map((i) =>
          this.key(i.garmentId, i.serviceId) === key
            ? { ...i, quantity: i.quantity + item.quantity, weightKg: item.weightKg ?? i.weightKg }
            : i
        )
      );
    } else {
      this._items.update((items) => [...items, item]);
    }
  }

  updateQuantity(garmentId: string, serviceId: string, quantity: number): void {
    if (quantity <= 0) {
      this.remove(garmentId, serviceId);
      return;
    }
    const key = this.key(garmentId, serviceId);
    this._items.update((items) =>
      items.map((i) => (this.key(i.garmentId, i.serviceId) === key ? { ...i, quantity } : i))
    );
  }

  remove(garmentId: string, serviceId: string): void {
    const key = this.key(garmentId, serviceId);
    this._items.update((items) => items.filter((i) => this.key(i.garmentId, i.serviceId) !== key));
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
