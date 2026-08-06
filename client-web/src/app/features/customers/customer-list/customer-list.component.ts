import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CustomersService } from '../customers.service';
import { CustomerListItem, MembershipTier } from '../../../core/models/customer.models';

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

  private loadCustomers(): void {
    this.isLoading.set(true);
    this.customersService.getCustomers({ search: this.search() || undefined }).subscribe({
      next: (result) => {
        this.customers.set(result.items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
