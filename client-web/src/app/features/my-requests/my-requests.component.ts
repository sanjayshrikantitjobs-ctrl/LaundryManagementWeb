import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdersService } from '../orders/orders.service';
import { OrderListItem, OrderStatus } from '../../core/models/order.models';
import { orderStatusLabel, TERMINAL_ORDER_STATUSES } from '../../core/utils/order-status.util';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-requests.component.html',
  styleUrl: './my-requests.component.scss'
})
export class MyRequestsComponent implements OnInit {
  private readonly ordersService = inject(OrdersService);

  private readonly allOrders = signal<OrderListItem[]>([]);
  readonly isLoading = signal(true);
  readonly orderStatusLabel = orderStatusLabel;

  readonly activeRequests = computed(() =>
    this.allOrders().filter((o) => !TERMINAL_ORDER_STATUSES.has(o.status))
  );

  readonly OrderStatus = OrderStatus;

  ngOnInit(): void {
    this.ordersService.getMyOrders({ pageSize: 100 }).subscribe({
      next: (result) => {
        this.allOrders.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
