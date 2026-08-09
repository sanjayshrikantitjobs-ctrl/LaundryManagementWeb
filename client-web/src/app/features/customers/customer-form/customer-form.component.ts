import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CustomersService } from '../customers.service';
import { MembershipTier } from '../../../core/models/customer.models';

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
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly membershipTiers = [
    { value: MembershipTier.None, label: 'None' },
    { value: MembershipTier.Silver, label: 'Silver' },
    { value: MembershipTier.Gold, label: 'Gold' }
  ];

  private customerId: string | null = null;

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.email]],
    creditLimit: [0, [Validators.required, Validators.min(0)]],
    membershipTier: [MembershipTier.None],
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
          membershipTier: customer.membershipTier,
          notes: customer.notes
        });
      });
    }
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
          membershipTier: value.membershipTier!,
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
