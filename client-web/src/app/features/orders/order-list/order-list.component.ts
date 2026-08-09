import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdersService } from '../orders.service';
import { OrderHubService } from '../../../core/services/order-hub.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderListItem, OrderStatus } from '../../../core/models/order.models';
import { IN_PROGRESS_ORDER_STATUSES, orderStatusLabel, paymentStatusLabel } from '../../../core/utils/order-status.util';
import { Subscription } from 'rxjs';

type OrderTab = 'all' | 'new' | 'progress' | 'completed' | 'cancelled';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);

  readonly orders = signal<OrderListItem[]>([]);
  readonly isLoading = signal(true);
  readonly isCustomer = this.authService.currentUser()?.role === 'Customer';
  readonly OrderStatus = OrderStatus;

  readonly activeTab = signal<OrderTab>('all');
  readonly tabCounts = computed(() => {
    const list = this.orders();
    return {
      all: list.length,
      new: list.filter((o) => o.status === OrderStatus.New).length,
      progress: list.filter((o) => IN_PROGRESS_ORDER_STATUSES.has(o.status)).length,
      completed: list.filter((o) => o.status === OrderStatus.Delivered).length,
      cancelled: list.filter((o) => o.status === OrderStatus.Cancelled).length
    };
  });

  readonly filteredOrders = computed(() => {
    const tab = this.activeTab();
    const list = this.orders();
    switch (tab) {
      case 'new':
        return list.filter((o) => o.status === OrderStatus.New);
      case 'progress':
        return list.filter((o) => IN_PROGRESS_ORDER_STATUSES.has(o.status));
      case 'completed':
        return list.filter((o) => o.status === OrderStatus.Delivered);
      case 'cancelled':
        return list.filter((o) => o.status === OrderStatus.Cancelled);
      default:
        return list;
    }
  });

  private hubSub?: Subscription;

  constructor(private ordersService: OrdersService, private orderHub: OrderHubService) {}

  async ngOnInit(): Promise<void> {
    this.loadOrders();

    await this.orderHub.connect();
    await this.orderHub.joinDashboardGroup();
    this.hubSub = this.orderHub.statusUpdates$.subscribe((update) => {
      this.orders.update((list) =>
        list.map((o) => (o.id === update.orderId ? { ...o, status: update.newStatus } : o))
      );
    });
  }

  ngOnDestroy(): void {
    this.hubSub?.unsubscribe();
    this.orderHub.disconnect();
  }

  private loadOrders(): void {
    this.isLoading.set(true);
    const request$ = this.isCustomer
      ? this.ordersService.getMyOrders({ pageSize: 100 })
      : this.ordersService.getOrders({ pageSize: 100 });
    request$.subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  statusLabel = orderStatusLabel;
  paymentStatusLabel = paymentStatusLabel;
}
