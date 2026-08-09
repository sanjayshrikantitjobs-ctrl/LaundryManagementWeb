import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OrdersService } from '../orders.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderDetail, OrderStatus, PaymentStatus } from '../../../core/models/order.models';
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
  private readonly ordersService = inject(OrdersService);
  private readonly authService = inject(AuthService);

  readonly order = signal<OrderDetail | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly orderStatusLabel = orderStatusLabel;
  readonly paymentStatusLabel = paymentStatusLabel;

  readonly isManagement = ['Admin', 'StoreManager', 'Staff'].includes(this.authService.currentUser()?.role ?? '');
  readonly statusOptions = Object.entries(ORDER_STATUS_LABELS).map(([value, label]) => ({ value: +value as OrderStatus, label }));
  readonly paymentStatusOptions = Object.entries(PAYMENT_STATUS_LABELS).map(([value, label]) => ({ value: +value as PaymentStatus, label }));

  readonly editStatus = signal<OrderStatus | null>(null);
  readonly editPaymentStatus = signal<PaymentStatus | null>(null);
  readonly editAmountPaid = signal<number | null>(null);
  readonly editPromisedAt = signal('');
  readonly isSaving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly saved = signal(false);

  private orderId: string | null = null;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id');
    if (!this.orderId) return;

    this.load(this.orderId);
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
