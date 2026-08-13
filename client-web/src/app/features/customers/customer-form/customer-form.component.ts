import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomersService } from '../customers.service';
import { SubscriptionsService } from '../../subscriptions/subscriptions.service';
import { BillingCycle, CustomerSubscriptionListItem, SubscriptionPlan, SubscriptionStatus } from '../../../core/models/subscription.models';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss'
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly customersService = inject(CustomersService);
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly SubscriptionStatus = SubscriptionStatus;
  readonly subscriptionStatuses = [
    { value: SubscriptionStatus.Active, label: 'Active' },
    { value: SubscriptionStatus.Paused, label: 'Paused' },
    { value: SubscriptionStatus.Cancelled, label: 'Cancelled' },
    { value: SubscriptionStatus.Expired, label: 'Expired' }
  ];
  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly currentSubscription = signal<CustomerSubscriptionListItem | null>(null);
  readonly isSavingMembership = signal(false);
  readonly membershipError = signal<string | null>(null);
  readonly membershipSaved = signal(false);

  readonly membershipForm = this.fb.group({
    subscriptionPlanId: ['', Validators.required],
    recurringValue: [0, [Validators.required, Validators.min(0)]],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required]
  });

  readonly statusForm = this.fb.group({
    status: [SubscriptionStatus.Active],
    recurringValue: [0, [Validators.required, Validators.min(0)]]
  });

  private customerId: string | null = null;

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.email]],
    creditLimit: [0, [Validators.required, Validators.min(0)]],
    notes: ['']
  });

  readonly newPassword = signal('');
  readonly isSavingPassword = signal(false);
  readonly passwordError = signal<string | null>(null);
  readonly passwordSaved = signal(false);

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id');
    if (this.customerId) {
      this.isEditMode.set(true);
      this.customersService.getCustomerById(this.customerId).subscribe((customer) => {
        this.form.patchValue({
          fullName: customer.fullName,
          phoneNumber: customer.phoneNumber,
          email: customer.email,
          creditLimit: customer.creditLimit,
          notes: customer.notes
        });
      });

      this.subscriptionsService.getPlans().subscribe((plans) => this.plans.set(plans.filter((p) => p.isActive)));
      this.loadCurrentSubscription();
    }
  }

  private loadCurrentSubscription(): void {
    if (!this.customerId) return;
    this.subscriptionsService.getCustomerSubscriptions({ customerId: this.customerId, pageSize: 1 }).subscribe((result) => {
      const sub = result.items[0] ?? null;
      this.currentSubscription.set(sub);
      if (sub) {
        this.statusForm.patchValue({ status: sub.status, recurringValue: sub.recurringValue });
      }
    });
  }

  subscriptionStatusLabel(status: SubscriptionStatus): string {
    return SubscriptionStatus[status];
  }

  billingCycleLabel(cycle: BillingCycle): string {
    return BillingCycle[cycle];
  }

  onPlanSelected(planId: string): void {
    const plan = this.plans().find((p) => p.id === planId);
    if (plan) this.membershipForm.patchValue({ recurringValue: plan.price });
  }

  assignMembership(): void {
    if (this.membershipForm.invalid || !this.customerId) {
      this.membershipForm.markAllAsTouched();
      return;
    }

    this.isSavingMembership.set(true);
    this.membershipError.set(null);
    const value = this.membershipForm.getRawValue();

    this.subscriptionsService
      .assignSubscription({
        customerId: this.customerId,
        subscriptionPlanId: value.subscriptionPlanId!,
        recurringValue: value.recurringValue!,
        startDate: value.startDate!
      })
      .subscribe({
        next: () => {
          this.isSavingMembership.set(false);
          this.membershipSaved.set(true);
          this.loadCurrentSubscription();
          setTimeout(() => this.membershipSaved.set(false), 2500);
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingMembership.set(false);
          this.membershipError.set(err.error?.title ?? 'Failed to assign membership.');
        }
      });
  }

  updateMembership(): void {
    const sub = this.currentSubscription();
    if (!sub || this.statusForm.invalid) return;

    this.isSavingMembership.set(true);
    this.membershipError.set(null);
    const value = this.statusForm.getRawValue();

    this.subscriptionsService
      .updateSubscription(sub.id, {
        recurringValue: value.recurringValue!,
        status: value.status!,
        nextBillingDate: sub.nextBillingDate
      })
      .subscribe({
        next: () => {
          this.isSavingMembership.set(false);
          this.membershipSaved.set(true);
          this.loadCurrentSubscription();
          setTimeout(() => this.membershipSaved.set(false), 2500);
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingMembership.set(false);
          this.membershipError.set(err.error?.title ?? 'Failed to update membership.');
        }
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();

    const request$: Observable<unknown> = this.customerId
      ? this.customersService.updateCustomer(this.customerId, {
          fullName: value.fullName!,
          phoneNumber: value.phoneNumber!,
          email: value.email || undefined,
          creditLimit: value.creditLimit!,
          notes: value.notes || undefined
        })
      : this.customersService.createCustomer({
          fullName: value.fullName!,
          phoneNumber: value.phoneNumber!,
          email: value.email || undefined,
          creditLimit: value.creditLimit!,
          notes: value.notes || undefined
        });

    request$.subscribe({
      next: () => this.router.navigate(['/customers']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save customer.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/customers']);
  }

  savePassword(): void {
    if (!this.customerId || this.newPassword().length < 8) {
      this.passwordError.set('Password must be at least 8 characters.');
      return;
    }

    this.isSavingPassword.set(true);
    this.passwordError.set(null);
    this.passwordSaved.set(false);

    this.customersService.setPassword(this.customerId, this.newPassword()).subscribe({
      next: () => {
        this.isSavingPassword.set(false);
        this.passwordSaved.set(true);
        this.newPassword.set('');
        setTimeout(() => this.passwordSaved.set(false), 2500);
      },
      error: (err: HttpErrorResponse) => {
        this.isSavingPassword.set(false);
        this.passwordError.set(err.error?.title ?? 'Failed to update password.');
      }
    });
  }
}
