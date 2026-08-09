import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { CustomersService } from '../customers/customers.service';
import { CustomerAddress } from '../../core/models/customer.models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly customersService = inject(CustomersService);

  readonly activeTab = signal<'general' | 'address'>('general');

  readonly generalForm = this.fb.group({
    fullName: ['', Validators.required],
    email: [''],
    whatsAppNumber: ['']
  });
  readonly isSavingGeneral = signal(false);
  readonly generalSaved = signal(false);
  readonly generalError = signal<string | null>(null);

  readonly addresses = signal<CustomerAddress[]>([]);
  readonly isLoadingAddresses = signal(true);
  readonly addressesError = signal<string | null>(null);
  readonly showAddressForm = signal(false);
  readonly addressForm = this.fb.group({
    label: ['Home', Validators.required],
    line1: ['', Validators.required],
    line2: [''],
    city: ['', Validators.required],
    state: ['', Validators.required],
    postalCode: ['', Validators.required],
    isDefault: [false]
  });
  readonly isSavingAddress = signal(false);
  readonly profileError = signal<string | null>(null);

  ngOnInit(): void {
    this.customersService.getMyProfile().subscribe({
      next: (profile) => {
        this.generalForm.patchValue({
          fullName: profile.fullName,
          email: profile.email ?? '',
          whatsAppNumber: profile.whatsAppNumber ?? ''
        });
      },
      error: () => this.profileError.set('Could not load your profile. Try refreshing the page.')
    });

    this.loadAddresses();
  }

  loadAddresses(): void {
    this.isLoadingAddresses.set(true);
    this.addressesError.set(null);
    this.customersService.getMyAddresses().subscribe({
      next: (addresses) => {
        this.addresses.set(addresses);
        this.isLoadingAddresses.set(false);
      },
      error: () => {
        this.addressesError.set('Could not load your addresses. Try refreshing the page.');
        this.isLoadingAddresses.set(false);
      }
    });
  }

  saveGeneral(): void {
    if (this.generalForm.invalid) {
      this.generalForm.markAllAsTouched();
      return;
    }
    this.isSavingGeneral.set(true);
    this.generalSaved.set(false);
    this.generalError.set(null);

    const value = this.generalForm.getRawValue();
    this.customersService
      .updateMyProfile({
        fullName: value.fullName!,
        email: value.email || undefined,
        whatsAppNumber: value.whatsAppNumber || undefined
      })
      .subscribe({
        next: () => {
          this.isSavingGeneral.set(false);
          this.generalSaved.set(true);
          setTimeout(() => this.generalSaved.set(false), 2500);
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingGeneral.set(false);
          this.generalError.set(err.error?.title ?? 'Failed to update profile.');
        }
      });
  }

  toggleAddressForm(): void {
    this.showAddressForm.update((v) => !v);
    this.addressForm.reset({ label: 'Home', isDefault: this.addresses().length === 0 });
  }

  saveAddress(): void {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }
    this.isSavingAddress.set(true);
    const value = this.addressForm.getRawValue();

    this.customersService
      .addMyAddress({
        label: value.label!,
        line1: value.line1!,
        line2: value.line2 || undefined,
        city: value.city!,
        state: value.state!,
        postalCode: value.postalCode!,
        isDefault: this.addresses().length === 0 || !!value.isDefault
      })
      .subscribe({
        next: () => {
          this.isSavingAddress.set(false);
          this.showAddressForm.set(false);
          this.loadAddresses();
        },
        error: () => this.isSavingAddress.set(false)
      });
  }

  setPrimary(addressId: string): void {
    this.customersService.setPrimaryAddress(addressId).subscribe(() => this.loadAddresses());
  }

  deleteAddress(addressId: string): void {
    this.customersService.deleteMyAddress(addressId).subscribe(() => this.loadAddresses());
  }
}
