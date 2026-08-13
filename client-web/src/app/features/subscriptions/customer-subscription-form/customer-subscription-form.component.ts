import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubscriptionsService } from '../subscriptions.service';
import { CustomersService } from '../../customers/customers.service';
import { CustomerListItem } from '../../../core/models/customer.models';
import { SubscriptionPlan, SubscriptionStatus } from '../../../core/models/subscription.models';

@Component({
  selector: 'app-customer-subscription-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-subscription-form.component.html',
  styleUrl: './customer-subscription-form.component.scss'
})
export class CustomerSubscriptionFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly customersService = inject(CustomersService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly SubscriptionStatus = SubscriptionStatus;

  readonly selectedCustomer = signal<CustomerListItem | null>(null);
  readonly customerSearch = signal('');
  readonly customerResults = signal<CustomerListItem[]>([]);

  private subscriptionId: string | null = null;

  readonly form = this.fb.group({
    subscriptionPlanId: ['', [Validators.required]],
    recurringValue: [0, [Validators.required, Validators.min(0)]],
    startDate: [this.today(), [Validators.required]],
    status: [SubscriptionStatus.Active],
    notes: ['']
  });

  ngOnInit(): void {
    this.subscriptionsService.getPlans().subscribe((plans) => this.plans.set(plans));

    this.subscriptionId = this.route.snapshot.paramMap.get('id');
    if (this.subscriptionId) {
      this.isEditMode.set(true);
      this.subscriptionsService.getCustomerSubscriptionById(this.subscriptionId).subscribe((subscription) => {
        this.selectedCustomer.set({
          id: subscription.customerId,
          fullName: subscription.customerName
        } as CustomerListItem);
        this.form.patchValue({
          subscriptionPlanId: subscription.subscriptionPlanId,
          recurringValue: subscription.recurringValue,
          startDate: subscription.startDate,
          status: subscription.status,
          notes: subscription.notes
        });
      });
    }
  }

  searchCustomers(term: string): void {
    this.customerSearch.set(term);
    if (!term) {
      this.customerResults.set([]);
      return;
    }
    this.customersService.getCustomers({ search: term, pageSize: 10 }).subscribe((result) => {
      this.customerResults.set(result.items);
    });
  }

  selectCustomer(customer: CustomerListItem): void {
    this.selectedCustomer.set(customer);
    this.customerResults.set([]);
  }

  onPlanChange(planId: string): void {
    const plan = this.plans().find((p) => p.id === planId);
    if (plan && !this.isEditMode()) {
      this.form.patchValue({ recurringValue: plan.price });
    }
  }

  save(): void {
    const customer = this.selectedCustomer();
    if (!customer || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();

    const request$: Observable<unknown> = this.subscriptionId
      ? this.subscriptionsService.updateSubscription(this.subscriptionId, {
          recurringValue: value.recurringValue!,
          status: value.status!,
          notes: value.notes || undefined
        })
      : this.subscriptionsService.assignSubscription({
          customerId: customer.id,
          subscriptionPlanId: value.subscriptionPlanId!,
          recurringValue: value.recurringValue!,
          startDate: value.startDate!,
          notes: value.notes || undefined
        });

    request$.subscribe({
      next: () => this.router.navigate(['/subscriptions/customers']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save subscription.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/subscriptions/customers']);
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
