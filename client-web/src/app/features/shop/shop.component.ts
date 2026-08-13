import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ServicesService } from '../services/services.service';
import { GarmentsService } from '../garments/garments.service';
import { CustomersService } from '../customers/customers.service';
import { OrdersService } from '../orders/orders.service';
import { PromotionsService } from '../promotions/promotions.service';
import { CategoriesService } from '../categories/categories.service';
import { AddOnsService } from '../add-ons/add-ons.service';
import { CartService, CartItem, CartItemAddOn } from './cart.service';
import { GarmentIconComponent } from '../../shared/garment-icon/garment-icon.component';
import { AddOn, GarmentListItem, PricingType, ServiceCategory, ServiceListItem } from '../../core/models/catalog.models';
import { CustomerAddress } from '../../core/models/customer.models';
import { ActivePromotion } from '../../core/models/promotion.models';
import { OrderChannel } from '../../core/models/order.models';

interface PriceLookup {
  pricingType: PricingType;
  price: number;
  expressPrice: number | null;
  isActive: boolean;
}

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [CommonModule, FormsModule, GarmentIconComponent],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss'
})
export class ShopComponent implements OnInit {
  private readonly servicesService = inject(ServicesService);
  private readonly garmentsService = inject(GarmentsService);
  private readonly customersService = inject(CustomersService);
  private readonly ordersService = inject(OrdersService);
  private readonly promotionsService = inject(PromotionsService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly addOnsService = inject(AddOnsService);
  private readonly router = inject(Router);
  readonly cart = inject(CartService);

  readonly PricingType = PricingType;

  readonly promotions = signal<ActivePromotion[]>([]);
  readonly categories = signal<ServiceCategory[]>([]);
  readonly services = signal<ServiceListItem[]>([]);
  readonly garments = signal<GarmentListItem[]>([]);
  readonly addOns = signal<AddOn[]>([]);
  readonly priceLookup = signal<Map<string, PriceLookup>>(new Map());
  readonly addresses = signal<CustomerAddress[]>([]);

  readonly selectedCategory = signal<ServiceCategory | null>(null);
  readonly selectedService = signal<ServiceListItem | null>(null);
  readonly quantities = signal<Map<string, number>>(new Map());
  readonly selectedAddOnIds = signal<Map<string, Set<string>>>(new Map());
  readonly isCartOpen = signal(false);
  readonly isExpress = signal(false);
  readonly isSameDay = signal(false);
  readonly selectedAddressId = signal<string | null>(null);
  readonly pickupAt = signal<string>('');
  readonly isPlacingOrder = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);

  readonly showNewAddressForm = signal(false);
  readonly newAddress = signal({ label: 'Home', line1: '', line2: '', city: '', state: '', postalCode: '', isDefault: false });

  readonly promoCodeInput = signal('');
  readonly appliedPromo = signal<ActivePromotion | null>(null);
  readonly promoError = signal<string | null>(null);

  // Client-side preview only, based on the Service's flat surcharge — the
  // authoritative total (which also accounts for any per-item ExpressPrice
  // override) is computed server-side when the order is created.
  readonly expressExtra = computed(() =>
    this.isExpress()
      ? this.cart.items().reduce((sum, item) => sum + (item.expressSurcharge ?? 0) * item.quantity, 0)
      : 0
  );

  readonly discountAmount = computed(() => {
    const promo = this.appliedPromo();
    if (!promo) return 0;
    const subtotal = this.cart.subtotal();
    const discount = promo.discountPercent ? (subtotal * promo.discountPercent) / 100 : promo.discountAmount ?? 0;
    return Math.min(discount, subtotal);
  });

  readonly grandTotal = computed(() => this.cart.subtotal() + this.expressExtra() - this.discountAmount());

  ngOnInit(): void {
    this.promotionsService.getActivePromotions().subscribe((promos) => this.promotions.set(promos));
    this.addOnsService.getAddOns(true).subscribe((addOns) => this.addOns.set(addOns));

    this.categoriesService.getCategories().subscribe((categories) => {
      this.categories.set(categories.filter((c) => c.isActive).sort((a, b) => a.displayOrder - b.displayOrder));
      this.selectDefaultCategoryAndService();
    });

    this.servicesService.getServices({ pageSize: 50 }).subscribe((result) => {
      this.services.set(result.items);
      this.selectDefaultCategoryAndService();
    });

    this.garmentsService.getGarments({ pageSize: 200 }).subscribe((result) => this.garments.set(result.items));

    this.garmentsService.getPricingMatrix().subscribe((matrix) => {
      const lookup = new Map<string, PriceLookup>();
      for (const row of matrix.garments) {
        for (const cell of row.prices) {
          if (cell.price !== null && cell.pricingType !== null) {
            lookup.set(this.priceKey(row.garmentId, cell.serviceId), {
              pricingType: cell.pricingType,
              price: cell.price,
              expressPrice: cell.expressPrice,
              isActive: cell.isActive
            });
          }
        }
      }
      this.priceLookup.set(lookup);
      this.isLoading.set(false);
    });

    this.loadAddresses();
  }

  loadAddresses(): void {
    this.customersService.getMyAddresses().subscribe((addresses) => {
      this.addresses.set(addresses);
      const primary = addresses.find((a) => a.isDefault) ?? addresses[0];
      if (primary) this.selectedAddressId.set(primary.id);
    });
  }

  priceKey(garmentId: string, serviceId: string): string {
    return `${garmentId}:${serviceId}`;
  }

  servicesForSelectedCategory(): ServiceListItem[] {
    const category = this.selectedCategory();
    if (!category) return [];
    return this.services()
      .filter((s) => s.categoryId === category.id)
      .sort((a, b) => a.priority - b.priority);
  }

  selectCategory(category: ServiceCategory): void {
    this.selectedCategory.set(category);
    const firstService = this.servicesForSelectedCategory()[0];
    if (firstService) this.selectService(firstService);
  }

  // Categories and services load via two independent, unordered HTTP calls — this runs
  // after each resolves and only acts once both are in. Defaults to the first category
  // (by displayOrder) that actually has a service, not just categories()[0], since an
  // empty category would otherwise show an empty service/garment list on first load.
  private selectDefaultCategoryAndService(): void {
    if (this.selectedCategory() || this.categories().length === 0 || this.services().length === 0) return;

    const category = this.categories().find((c) => this.services().some((s) => s.categoryId === c.id));
    if (!category) return;

    this.selectedCategory.set(category);
    const firstService = this.servicesForSelectedCategory()[0];
    if (firstService) this.selectService(firstService);
  }

  selectService(service: ServiceListItem): void {
    this.selectedService.set(service);
  }

  priceFor(garmentId: string): PriceLookup | undefined {
    const service = this.selectedService();
    if (!service) return undefined;
    const price = this.priceLookup().get(this.priceKey(garmentId, service.id));
    return price?.isActive ? price : undefined;
  }

  garmentsForSelectedService(): GarmentListItem[] {
    const service = this.selectedService();
    if (!service) return [];
    return this.garments().filter((g) => this.priceFor(g.id));
  }

  quantityFor(garmentId: string): number {
    return this.quantities().get(garmentId) ?? 0;
  }

  canAddToCart(garmentId: string): boolean {
    return this.quantityFor(garmentId) > 0;
  }

  setQuantity(garmentId: string, qty: number): void {
    const clamped = Math.max(0, Math.trunc(qty) || 0);
    this.quantities.update((map) => new Map(map).set(garmentId, clamped));
  }

  isAddOnSelected(garmentId: string, addOnId: string): boolean {
    return this.selectedAddOnIds().get(garmentId)?.has(addOnId) ?? false;
  }

  toggleAddOn(garmentId: string, addOnId: string): void {
    this.selectedAddOnIds.update((map) => {
      const next = new Map(map);
      const current = new Set(next.get(garmentId) ?? []);
      if (current.has(addOnId)) current.delete(addOnId);
      else current.add(addOnId);
      next.set(garmentId, current);
      return next;
    });
  }

  addToCart(garment: GarmentListItem): void {
    const service = this.selectedService();
    const category = this.selectedCategory();
    const price = this.priceFor(garment.id);
    const quantity = this.quantityFor(garment.id);
    if (!service || !category || !price || quantity <= 0) return;

    const selectedIds = this.selectedAddOnIds().get(garment.id) ?? new Set<string>();
    const selectedAddOns: CartItemAddOn[] = this.addOns()
      .filter((a) => selectedIds.has(a.id))
      .map((a) => ({ id: a.id, name: a.name, price: a.price }));

    const item: CartItem = {
      garmentId: garment.id,
      garmentName: garment.name,
      garmentImageUrl: garment.imageUrl,
      categoryName: category.name,
      serviceId: service.id,
      serviceName: service.name,
      pricingType: price.pricingType,
      unitPrice: price.price,
      quantity: price.pricingType === PricingType.WeightBased ? 1 : quantity,
      weightKg: price.pricingType === PricingType.WeightBased ? quantity : undefined,
      expressSurcharge: service.expressSurcharge,
      selectedAddOns
    };
    this.cart.add(item);
    this.setQuantity(garment.id, 0);
    this.selectedAddOnIds.update((map) => {
      const next = new Map(map);
      next.delete(garment.id);
      return next;
    });
    this.isCartOpen.set(true);
  }

  lineTotal(item: CartItem): number {
    return this.cart.lineTotal(item);
  }

  addOnsSummary(item: CartItem): string {
    const names = (item.selectedAddOns ?? []).map((a) => a.name);
    return names.join(', ');
  }

  applyPromoCode(): void {
    const code = this.promoCodeInput().trim();
    if (!code) return;

    const match = this.promotions().find((p) => p.code?.toUpperCase() === code.toUpperCase());
    if (!match) {
      this.appliedPromo.set(null);
      this.promoError.set('That promo code is invalid or has expired.');
      return;
    }
    this.appliedPromo.set(match);
    this.promoError.set(null);
  }

  removePromoCode(): void {
    this.appliedPromo.set(null);
    this.promoCodeInput.set('');
    this.promoError.set(null);
  }

  toggleAddressForm(): void {
    this.showNewAddressForm.update((v) => !v);
  }

  saveNewAddress(): void {
    const addr = this.newAddress();
    if (!addr.line1 || !addr.city || !addr.state || !addr.postalCode) return;

    this.customersService
      .addMyAddress({
        label: addr.label || 'Home',
        line1: addr.line1,
        line2: addr.line2 || undefined,
        city: addr.city,
        state: addr.state,
        postalCode: addr.postalCode,
        isDefault: this.addresses().length === 0 || addr.isDefault
      })
      .subscribe((id) => {
        this.showNewAddressForm.set(false);
        this.newAddress.set({ label: 'Home', line1: '', line2: '', city: '', state: '', postalCode: '', isDefault: false });
        this.customersService.getMyAddresses().subscribe((addresses) => {
          this.addresses.set(addresses);
          this.selectedAddressId.set(id);
        });
      });
  }

  placeOrder(): void {
    if (this.cart.items().length === 0) {
      this.errorMessage.set('Your cart is empty.');
      return;
    }

    this.isPlacingOrder.set(true);
    this.errorMessage.set(null);

    this.customersService.getMyProfile().subscribe({
      next: (customer) => {
        this.ordersService
          .createOrder({
            customerId: customer.id,
            // Channel is purely the fulfillment method (Shop always books a pickup);
            // IsExpress alone drives express pricing/ETA server-side.
            channel: OrderChannel.PickupRequest,
            isExpress: this.isExpress(),
            isSameDay: this.isSameDay(),
            items: this.cart.items().map((i) => ({
              garmentId: i.garmentId,
              serviceId: i.serviceId,
              quantity: i.quantity,
              weightKg: i.weightKg,
              addOnIds: (i.selectedAddOns ?? []).map((a) => a.id)
            })),
            preferredPickupAtUtc: this.pickupAt() ? new Date(this.pickupAt()).toISOString() : undefined,
            pickupAddressId: this.selectedAddressId() ?? undefined,
            promoCode: this.appliedPromo()?.code ?? undefined
          })
          .subscribe({
            next: () => {
              this.cart.clear();
              this.removePromoCode();
              this.isPlacingOrder.set(false);
              this.router.navigate(['/my-requests']);
            },
            error: (err: HttpErrorResponse) => {
              this.isPlacingOrder.set(false);
              this.errorMessage.set(err.error?.title ?? 'Failed to place order.');
            }
          });
      },
      error: () => {
        this.isPlacingOrder.set(false);
        this.errorMessage.set('Could not load your profile.');
      }
    });
  }
}
