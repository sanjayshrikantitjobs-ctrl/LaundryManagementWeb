import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomersService } from '../customers.service';
import { CustomerListItem, CustomerStatus } from '../../../core/models/customer.models';
import { SortDirection } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { SubscriptionsService } from '../../subscriptions/subscriptions.service';
import { CustomerSubscriptionListItem, SubscriptionStatus } from '../../../core/models/subscription.models';

type CustomerTab = 'all' | 'new' | 'inactive' | 'subscribed';
type ActivityTier = 'new' | 'active' | 'yellow' | 'orange' | 'red';

const DAY_MS = 24 * 60 * 60 * 1000;

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent implements OnInit {
  private readonly customersService = inject(CustomersService);
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly customers = signal<CustomerListItem[]>([]);
  readonly subscribedCustomers = signal<CustomerSubscriptionListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly CustomerStatus = CustomerStatus;
  readonly SubscriptionStatus = SubscriptionStatus;
  readonly activeTab = signal<CustomerTab>('all');

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('asc');

  ngOnInit(): void {
    this.loadCustomers();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadCustomers();
  }

  setTab(tab: CustomerTab): void {
    this.activeTab.set(tab);
    this.pageNumber.set(1);
    this.loadCustomers();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadCustomers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadCustomers();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadCustomers();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  async deleteCustomer(customer: CustomerListItem): Promise<void> {
    const result = await this.confirmDialog.confirm({
      title: 'Delete customer',
      message: `Delete customer "${customer.fullName}"? This cannot be undone.`,
      requireReason: true,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!result.confirmed) return;

    this.customersService.deleteCustomer(customer.id, result.reason).subscribe({
      next: () => this.loadCustomers()
    });
  }

  async toggleStatus(customer: CustomerListItem): Promise<void> {
    const nextStatus = customer.status === CustomerStatus.Active ? CustomerStatus.Inactive : CustomerStatus.Active;
    const action = nextStatus === CustomerStatus.Inactive ? 'Deactivate' : 'Activate';
    const result = await this.confirmDialog.confirm({
      title: `${action} customer`,
      message: `${action} customer "${customer.fullName}"? Their orders and history will not be affected.`,
      confirmLabel: action
    });
    if (!result.confirmed) return;

    this.customersService.setStatus(customer.id, nextStatus).subscribe({
      next: () => this.loadCustomers()
    });
  }

  subscriptionStatusLabel(status: SubscriptionStatus): string {
    return SubscriptionStatus[status];
  }

  activityTier(customer: CustomerListItem): ActivityTier {
    if (!customer.lastOrderAtUtc) return 'new';

    const daysSince = (Date.now() - new Date(customer.lastOrderAtUtc).getTime()) / DAY_MS;
    if (daysSince < 7) return 'active';
    if (daysSince < 14) return 'yellow';
    if (daysSince < 21) return 'orange';
    return 'red';
  }

  activityLabel(customer: CustomerListItem): string {
    const tier = this.activityTier(customer);
    switch (tier) {
      case 'new':
        return 'No orders yet';
      case 'active':
        return 'Active';
      case 'yellow':
        return '1+ week inactive';
      case 'orange':
        return '2+ weeks inactive';
      case 'red':
        return '3+ weeks inactive';
    }
  }

  private loadCustomers(): void {
    this.isLoading.set(true);
    const tab = this.activeTab();

    if (tab === 'subscribed') {
      this.subscriptionsService
        .getCustomerSubscriptions({
          search: this.search() || undefined,
          pageNumber: this.pageNumber(),
          pageSize: this.pageSize()
        })
        .subscribe({
          next: (result) => {
            this.subscribedCustomers.set(result.items);
            this.totalCount.set(result.totalCount);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
        });
      return;
    }

    this.customersService
      .getCustomers({
        search: this.search() || undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy() ?? undefined,
        sortDirection: this.sortDirection(),
        hasOrders: tab === 'new' ? false : undefined,
        status: tab === 'inactive' ? CustomerStatus.Inactive : undefined
      })
      .subscribe({
        next: (result) => {
          this.customers.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
