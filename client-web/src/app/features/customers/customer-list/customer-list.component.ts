import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomersService } from '../customers.service';
import { CustomerListItem, MembershipTier } from '../../../core/models/customer.models';

type CustomerTab = 'all' | 'new';
type ActivityTier = 'new' | 'active' | 'yellow' | 'orange' | 'red';

const DAY_MS = 24 * 60 * 60 * 1000;

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent implements OnInit {
  readonly customers = signal<CustomerListItem[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly MembershipTier = MembershipTier;
  readonly activeTab = signal<CustomerTab>('all');

  readonly tabCounts = computed(() => {
    const list = this.customers();
    return {
      all: list.length,
      new: list.filter((c) => !c.lastOrderAtUtc).length
    };
  });

  readonly filteredCustomers = computed(() => {
    const list = this.customers();
    return this.activeTab() === 'new' ? list.filter((c) => !c.lastOrderAtUtc) : list;
  });

  constructor(private customersService: CustomersService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.loadCustomers();
  }

  deleteCustomer(customer: CustomerListItem): void {
    if (!confirm(`Delete customer "${customer.fullName}"?`)) return;

    this.customersService.deleteCustomer(customer.id).subscribe({
      next: () => this.loadCustomers()
    });
  }

  membershipLabel(tier: MembershipTier): string {
    return MembershipTier[tier];
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
    this.customersService.getCustomers({ search: this.search() || undefined, pageSize: 200 }).subscribe({
      next: (result) => {
        this.customers.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
