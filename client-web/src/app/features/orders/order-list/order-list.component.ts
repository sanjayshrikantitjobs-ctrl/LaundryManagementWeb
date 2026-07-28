import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdersService } from '../orders.service';
import { OrderHubService } from '../../../core/services/order-hub.service';
import { OrderListItem, OrderStatus } from '../../../core/models/order.models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit, OnDestroy {
  readonly orders = signal<OrderListItem[]>([]);
  readonly isLoading = signal(true);
  readonly OrderStatus = OrderStatus;

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
    this.ordersService.getOrders().subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  statusLabel(status: OrderStatus): string {
    return OrderStatus[status];
  }
}
