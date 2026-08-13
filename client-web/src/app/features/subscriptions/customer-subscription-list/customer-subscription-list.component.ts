import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SubscriptionsService } from '../subscriptions.service';
import { CustomerSubscriptionListItem, SubscriptionStatus } from '../../../core/models/subscription.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-customer-subscription-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, PaginationComponent],
  templateUrl: './customer-subscription-list.component.html',
  styleUrl: './customer-subscription-list.component.scss'
})
export class CustomerSubscriptionListComponent implements OnInit {
  private readonly subscriptionsService = inject(SubscriptionsService);

  readonly subscriptions = signal<CustomerSubscriptionListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly SubscriptionStatus = SubscriptionStatus;

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  ngOnInit(): void {
    this.loadSubscriptions();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadSubscriptions();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadSubscriptions();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadSubscriptions();
  }

  statusLabel(status: SubscriptionStatus): string {
    return SubscriptionStatus[status];
  }

  private loadSubscriptions(): void {
    this.isLoading.set(true);
    this.subscriptionsService
      .getCustomerSubscriptions({
        search: this.search() || undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize()
      })
      .subscribe({
        next: (result) => {
          this.subscriptions.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
