import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OrdersService } from '../orders.service';
import { CustomersService } from '../../customers/customers.service';
import { GarmentsService } from '../../garments/garments.service';
import { ServicesService } from '../../services/services.service';
import { OrderChannel } from '../../../core/models/order.models';
import { CustomerListItem } from '../../../core/models/customer.models';
import { GarmentListItem, PricingType, ServiceListItem } from '../../../core/models/catalog.models';

interface PriceLookup {
  pricingType: PricingType;
  price: number;
}

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.scss'
})
export class OrderFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly ordersService = inject(OrdersService);
  private readonly customersService = inject(CustomersService);
  private readonly garmentsService = inject(GarmentsService);
  private readonly servicesService = inject(ServicesService);
  private readonly router = inject(Router);

  readonly OrderChannel = OrderChannel;
  readonly PricingType = PricingType;

  readonly customerSearch = signal('');
  readonly customerResults = signal<CustomerListItem[]>([]);
  readonly selectedCustomer = signal<CustomerListItem | null>(null);

  readonly garments = signal<GarmentListItem[]>([]);
  readonly services = signal<ServiceListItem[]>([]);
  readonly priceLookup = signal<Map<string, PriceLookup>>(new Map());

  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.group({
    channel: [OrderChannel.WalkIn],
    isExpress: [false],
    items: this.fb.array([this.buildItemGroup()])
  });

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  // A plain method (not a computed signal) so it's re-evaluated on every change
  // detection pass — the reactive form's value changes aren't signals, so a
  // computed() here would only react to priceLookup updates, not item edits.
  subtotal(): number {
    return this.items.controls.reduce((sum, group) => sum + (this.lineTotal(group) ?? 0), 0);
  }

  ngOnInit(): void {
    this.garmentsService.getGarments({ pageSize: 200 }).subscribe((result) => this.garments.set(result.items));
    this.servicesService.getServices({ pageSize: 200 }).subscribe((result) => this.services.set(result.items));
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
    });
    this.searchCustomers('');
  }

  priceKey(garmentId: string, serviceId: string): string {
    return `${garmentId}:${serviceId}`;
  }

  priceFor(group: (typeof this.items.controls)[number]): PriceLookup | undefined {
    const { garmentId, serviceId } = group.value;
    if (!garmentId || !serviceId) return undefined;
    return this.priceLookup().get(this.priceKey(garmentId, serviceId));
  }

  lineTotal(group: (typeof this.items.controls)[number]): number | null {
    const price = this.priceFor(group);
    if (!price) return null;
    const { quantity, weightKg } = group.value;
    return price.pricingType === PricingType.WeightBased ? price.price * (weightKg || 0) : price.price * (quantity || 0);
  }

  searchCustomers(term: string): void {
    this.customerSearch.set(term);
    this.customersService.getCustomers({ search: term || undefined, pageSize: 10 }).subscribe((result) => {
      this.customerResults.set(result.items);
    });
  }

  selectCustomer(customer: CustomerListItem): void {
    this.selectedCustomer.set(customer);
    this.customerResults.set([]);
  }

  addItem(): void {
    this.items.push(this.buildItemGroup());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) this.items.removeAt(index);
  }

  save(): void {
    const customer = this.selectedCustomer();
    if (!customer || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();

    this.ordersService
      .createOrder({
        customerId: customer.id,
        channel: value.channel!,
        isExpress: value.isExpress!,
        items: value.items!.map((item) => ({
          garmentId: item.garmentId!,
          serviceId: item.serviceId!,
          quantity: item.quantity!,
          weightKg: item.weightKg || undefined,
          specialInstructions: item.specialInstructions || undefined
        }))
      })
      .subscribe({
        next: () => this.router.navigate(['/orders']),
        error: (err: HttpErrorResponse) => {
          this.isSaving.set(false);
          this.errorMessage.set(err.error?.title ?? 'Failed to create order.');
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/orders']);
  }

  private buildItemGroup() {
    return this.fb.group({
      garmentId: ['', Validators.required],
      serviceId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      weightKg: [null as number | null],
      specialInstructions: ['']
    });
  }
}
