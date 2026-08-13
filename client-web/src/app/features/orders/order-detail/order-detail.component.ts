import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OrdersService } from '../orders.service';
import { AuthService } from '../../../core/services/auth.service';
import { UploadService } from '../../../core/services/upload.service';
import { OrderGarmentImagesService } from '../order-garment-images.service';
import { PickupDeliveryService } from '../../pickup-delivery/pickup-delivery.service';
import { OrderDetail, OrderItemDetail, OrderStatus, PaymentStatus } from '../../../core/models/order.models';
import { GarmentImageType, OrderGarmentImage } from '../../../core/models/order-garment-image.models';
import { Agent, OrderPickupDelivery } from '../../../core/models/pickup-delivery.models';
import { UserRole } from '../../../core/models/user.models';
import { ORDER_STATUS_LABELS, orderStatusLabel, PAYMENT_STATUS_LABELS, paymentStatusLabel } from '../../../core/utils/order-status.util';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss'
})
export class OrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ordersService = inject(OrdersService);
  private readonly authService = inject(AuthService);
  private readonly uploadService = inject(UploadService);
  private readonly imagesService = inject(OrderGarmentImagesService);
  private readonly pickupDeliveryService = inject(PickupDeliveryService);

  readonly order = signal<OrderDetail | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly orderStatusLabel = orderStatusLabel;
  readonly paymentStatusLabel = paymentStatusLabel;

  addOnNames(item: OrderItemDetail): string {
    return item.addOns.map((a) => a.name).join(', ');
  }

  private readonly role = this.authService.currentUser()?.role ?? '';
  readonly isManagement = ['Admin', 'StoreManager', 'Staff'].includes(this.role);
  readonly canViewOperations = ['Admin', 'StoreManager', 'Staff', 'DepartmentHead'].includes(this.role);
  readonly isPickupAgent = this.role === 'PickupAgent';
  readonly isDeliveryAgent = this.role === 'DeliveryAgent';
  readonly statusOptions = Object.entries(ORDER_STATUS_LABELS).map(([value, label]) => ({ value: +value as OrderStatus, label }));
  readonly paymentStatusOptions = Object.entries(PAYMENT_STATUS_LABELS).map(([value, label]) => ({ value: +value as PaymentStatus, label }));

  readonly editStatus = signal<OrderStatus | null>(null);
  readonly editPaymentStatus = signal<PaymentStatus | null>(null);
  readonly editAmountPaid = signal<number | null>(null);
  readonly editPromisedAt = signal('');
  readonly isSaving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly saved = signal(false);

  // --- Garment Details (images) ---
  readonly GarmentImageType = GarmentImageType;
  readonly images = signal<OrderGarmentImage[]>([]);
  readonly imageTypeTab = signal<GarmentImageType>(GarmentImageType.Pickup);
  readonly serviceFilter = signal<string | null>(null);
  readonly isUploading = signal(false);
  readonly uploadNotes = signal('');
  readonly uploadError = signal<string | null>(null);

  readonly filteredImages = computed(() =>
    this.images().filter(
      (i) => i.imageType === this.imageTypeTab() && (this.serviceFilter() === null || i.serviceId === this.serviceFilter())
    )
  );

  readonly serviceTabs = computed(() => {
    const seen = new Map<string, string>();
    for (const item of this.order()?.items ?? []) seen.set(item.serviceId, item.serviceName);
    return Array.from(seen, ([serviceId, serviceName]) => ({ serviceId, serviceName }));
  });

  readonly canUpload = computed(() => {
    if (this.canViewOperations) return true;
    if (this.isPickupAgent) return this.imageTypeTab() === GarmentImageType.Pickup && !!this.pickupId();
    if (this.isDeliveryAgent) return this.imageTypeTab() === GarmentImageType.Delivery && !!this.deliveryId();
    return false;
  });

  // --- Pickup/Delivery confirmation + assignment ---
  readonly pickupDeliveries = signal<OrderPickupDelivery[]>([]);
  readonly pickupAgents = signal<Agent[]>([]);
  readonly deliveryAgents = signal<Agent[]>([]);
  readonly selectedAgentByLeg = signal<Record<string, string>>({});
  readonly isConfirming = signal(false);
  readonly confirmError = signal<string | null>(null);

  readonly pickupId = signal<string | null>(null);
  readonly deliveryId = signal<string | null>(null);

  private orderId: string | null = null;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id');
    this.pickupId.set(this.route.snapshot.queryParamMap.get('pickupId'));
    this.deliveryId.set(this.route.snapshot.queryParamMap.get('deliveryId'));
    if (this.deliveryId()) this.imageTypeTab.set(GarmentImageType.Delivery);
    if (!this.orderId) return;

    this.load(this.orderId);
    this.loadImages(this.orderId);

    if (this.canViewOperations) {
      this.loadPickupDeliveries(this.orderId);
    }
  }

  setImageTypeTab(type: GarmentImageType): void {
    this.imageTypeTab.set(type);
  }

  setServiceFilter(serviceId: string | null): void {
    this.serviceFilter.set(serviceId);
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file || !this.orderId) return;

    this.isUploading.set(true);
    this.uploadError.set(null);

    this.uploadService.uploadImage(file).subscribe({
      next: (uploaded) => {
        this.imagesService
          .addImage(this.orderId!, {
            imageType: this.imageTypeTab(),
            imageUrl: uploaded.url,
            serviceId: this.serviceFilter() ?? undefined,
            notes: this.uploadNotes() || undefined
          })
          .subscribe({
            next: () => {
              this.isUploading.set(false);
              this.uploadNotes.set('');
              this.loadImages(this.orderId!);
            },
            error: (err: HttpErrorResponse) => {
              this.isUploading.set(false);
              this.uploadError.set(err.error?.title ?? 'Failed to attach the uploaded image.');
            }
          });
      },
      error: () => {
        this.isUploading.set(false);
        this.uploadError.set('Failed to upload the image.');
      }
    });
  }

  deleteImage(image: OrderGarmentImage): void {
    if (!this.orderId || !confirm('Remove this image?')) return;

    this.imagesService.deleteImage(this.orderId, image.id).subscribe({
      next: () => this.loadImages(this.orderId!)
    });
  }

  confirmPickup(): void {
    const id = this.pickupId();
    if (!id) return;

    this.isConfirming.set(true);
    this.confirmError.set(null);
    this.pickupDeliveryService.confirmPickup(id).subscribe({
      next: () => this.router.navigate(['/pickup-queue']),
      error: (err: HttpErrorResponse) => {
        this.isConfirming.set(false);
        this.confirmError.set(err.error?.title ?? err.error ?? 'Failed to confirm pickup.');
      }
    });
  }

  confirmDelivery(): void {
    const id = this.deliveryId();
    if (!id) return;

    this.isConfirming.set(true);
    this.confirmError.set(null);
    this.pickupDeliveryService.confirmDelivery(id).subscribe({
      next: () => this.router.navigate(['/delivery-queue']),
      error: (err: HttpErrorResponse) => {
        this.isConfirming.set(false);
        this.confirmError.set(err.error?.title ?? err.error ?? 'Failed to confirm delivery.');
      }
    });
  }

  onAgentSelect(legId: string, employeeId: string): void {
    this.selectedAgentByLeg.update((map) => ({ ...map, [legId]: employeeId }));
  }

  assignAgent(leg: OrderPickupDelivery): void {
    const employeeId = this.selectedAgentByLeg()[leg.id];
    if (!employeeId) return;

    this.pickupDeliveryService.assignAgent(leg.id, employeeId).subscribe({
      next: () => this.loadPickupDeliveries(this.orderId!)
    });
  }

  agentsFor(leg: OrderPickupDelivery): Agent[] {
    return leg.isPickup ? this.pickupAgents() : this.deliveryAgents();
  }

  private loadImages(orderId: string): void {
    this.imagesService.getImages(orderId).subscribe({
      next: (images) => this.images.set(images)
    });
  }

  private loadPickupDeliveries(orderId: string): void {
    this.pickupDeliveryService.getForOrder(orderId).subscribe({
      next: (legs) => this.pickupDeliveries.set(legs)
    });

    if (this.isManagement) {
      this.pickupDeliveryService.getAgents(UserRole.PickupAgent).subscribe({ next: (a) => this.pickupAgents.set(a) });
      this.pickupDeliveryService.getAgents(UserRole.DeliveryAgent).subscribe({ next: (a) => this.deliveryAgents.set(a) });
    }
  }

  private load(orderId: string): void {
    this.ordersService.getOrderById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.editStatus.set(order.status);
        this.editPaymentStatus.set(order.paymentStatus);
        this.editAmountPaid.set(order.amountPaid);
        this.editPromisedAt.set(this.toDatetimeLocal(order.promisedByUtc));
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load this order.');
        this.isLoading.set(false);
      }
    });
  }

  saveChanges(): void {
    if (!this.orderId) return;

    this.isSaving.set(true);
    this.saveError.set(null);
    this.saved.set(false);

    this.ordersService
      .updateOrder(this.orderId, {
        status: this.editStatus() ?? undefined,
        paymentStatus: this.editPaymentStatus() ?? undefined,
        amountPaid: this.editAmountPaid() ?? undefined,
        promisedByUtc: this.editPromisedAt() ? new Date(this.editPromisedAt()).toISOString() : undefined
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.saved.set(true);
          this.load(this.orderId!);
          setTimeout(() => this.saved.set(false), 2500);
        },
        error: (err: HttpErrorResponse) => {
          this.isSaving.set(false);
          this.saveError.set(err.error?.title ?? 'Failed to update order.');
        }
      });
  }

  private toDatetimeLocal(iso?: string | null): string {
    if (!iso) return '';
    const d = new Date(iso);
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}
