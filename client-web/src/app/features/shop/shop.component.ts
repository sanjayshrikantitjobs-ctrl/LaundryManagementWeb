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
import { CartService, CartItem } from './cart.service';
import { GarmentIconComponent } from '../../shared/garment-icon/garment-icon.component';
import { GarmentListItem, PricingType, ServiceListItem } from '../../core/models/catalog.models';
import { CustomerAddress } from '../../core/models/customer.models';
import { ActivePromotion } from '../../core/models/promotion.models';
import { OrderChannel } from '../../core/models/order.models';

interface PriceLookup {
  pricingType: PricingType;
  price: number;
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
  private readonly router = inject(Router);
  readonly cart = inject(CartService);

  readonly PricingType = PricingType;

  readonly promotions = signal<ActivePromotion[]>([]);
  readonly services = signal<ServiceListItem[]>([]);
  readonly garments = signal<GarmentListItem[]>([]);
  readonly priceLookup = signal<Map<string, PriceLookup>>(new Map());
  readonly addresses = signal<CustomerAddress[]>([]);

  readonly selectedService = signal<ServiceListItem | null>(null);
  readonly quantities = signal<Map<string, number>>(new Map());
  readonly isCartOpen = signal(false);
  readonly isExpress = signal(false);
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

    this.servicesService.getServices({ pageSize: 50 }).subscribe((result) => {
      this.services.set(result.items);
      if (result.items.length) this.selectService(result.items[0]);
    });

    this.garmentsService.getGarments({ pageSize: 200 }).subscribe((result) => this.garments.set(result.items));

    this.garmentsService.getPricingMatrix().subscribe((matrix) => {
      const lookup = new Map<string, PriceLookup>();
      for (const row of matrix.garments) {
        for (const cell of row.prices) {
          if (cell.price !== null && cell.pricingType !== null) {
            lookup.set(this.priceKey(row.garmentId, cell.serviceId), { pricingType: cell.pricingType, price: cell.price });
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

  selectService(service: ServiceListItem): void {
    this.selectedService.set(service);
  }

  priceFor(garmentId: string): PriceLookup | undefined {
    const service = this.selectedService();
    if (!service) return undefined;
    return this.priceLookup().get(this.priceKey(garmentId, service.id));
  }

  garmentsForSelectedService(): GarmentListItem[] {
    const service = this.selectedService();
    if (!service) return [];
    return this.garments().filter((g) => this.priceLookup().has(this.priceKey(g.id, service.id)));
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

  addToCart(garment: GarmentListItem): void {
    const service = this.selectedService();
    const price = this.priceFor(garment.id);
    const quantity = this.quantityFor(garment.id);
    if (!service || !price || quantity <= 0) return;

    const item: CartItem = {
      garmentId: garment.id,
      garmentName: garment.name,
      garmentImageUrl: garment.imageUrl,
      garmentCategory: garment.category,
      serviceId: service.id,
      serviceName: service.name,
      pricingType: price.pricingType,
      unitPrice: price.price,
      quantity: price.pricingType === PricingType.WeightBased ? 1 : quantity,
      weightKg: price.pricingType === PricingType.WeightBased ? quantity : undefined,
      expressSurcharge: service.expressSurcharge
    };
    this.cart.add(item);
    this.setQuantity(garment.id, 0);
    this.isCartOpen.set(true);
  }

  lineTotal(item: CartItem): number {
    return this.cart.lineTotal(item);
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
            channel: this.isExpress() ? OrderChannel.Express : OrderChannel.PickupRequest,
            isExpress: this.isExpress(),
            items: this.cart.items().map((i) => ({
              garmentId: i.garmentId,
              serviceId: i.serviceId,
              quantity: i.quantity,
              weightKg: i.weightKg
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
