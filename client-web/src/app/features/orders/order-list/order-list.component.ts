import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrdersService } from '../orders.service';
import { OrderHubService } from '../../../core/services/order-hub.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderListItem, OrderStatus, SortDirection } from '../../../core/models/order.models';
import { IN_PROGRESS_ORDER_STATUSES, orderStatusLabel, paymentStatusLabel } from '../../../core/utils/order-status.util';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { Subscription } from 'rxjs';

type OrderTab = 'all' | 'new' | 'progress' | 'completed' | 'cancelled';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit, OnDestroy {
  private readonly authService = inject(AuthService);

  readonly orders = signal<OrderListItem[]>([]);
  readonly isLoading = signal(true);
  readonly totalCount = signal(0);
  readonly isCustomer = this.authService.currentUser()?.role === 'Customer';
  readonly OrderStatus = OrderStatus;

  readonly activeTab = signal<OrderTab>('all');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('desc');

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

  setTab(tab: OrderTab): void {
    this.activeTab.set(tab);
    this.pageNumber.set(1);
    this.loadOrders();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadOrders();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadOrders();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadOrders();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  private loadOrders(): void {
    this.isLoading.set(true);

    const tabParams = this.tabToParams();
    const opts = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy() ?? undefined,
      sortDirection: this.sortDirection(),
      ...tabParams
    };

    const request$ = this.isCustomer ? this.ordersService.getMyOrders(opts) : this.ordersService.getOrders(opts);
    request$.subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private tabToParams(): { status?: OrderStatus; statuses?: string } {
    switch (this.activeTab()) {
      case 'new':
        return { status: OrderStatus.New };
      case 'progress':
        return { statuses: Array.from(IN_PROGRESS_ORDER_STATUSES).join(',') };
      case 'completed':
        return { status: OrderStatus.Delivered };
      case 'cancelled':
        return { status: OrderStatus.Cancelled };
      default:
        return {};
    }
  }

  statusLabel = orderStatusLabel;
  paymentStatusLabel = paymentStatusLabel;
}
