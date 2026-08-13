import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly step = signal<'request' | 'reset'>('request');
  readonly devOtpCode = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly notFoundMessage = signal<string | null>(null);

  readonly requestForm = this.fb.nonNullable.group({
    usernameOrEmail: ['', Validators.required]
  });

  readonly resetForm = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  requestCode(): void {
    if (this.requestForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.notFoundMessage.set(null);

    this.auth.forgotPassword(this.requestForm.getRawValue()).subscribe({
      next: (result) => {
        this.isSubmitting.set(false);
        if (!result.accountFound) {
          this.notFoundMessage.set(
            'No account with a verified phone number matches that username/email — contact support for help.'
          );
          return;
        }
        this.devOtpCode.set(result.devOtpCode ?? null);
        if (result.devOtpCode) {
          this.resetForm.patchValue({ code: result.devOtpCode });
        }
        this.step.set('reset');
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('Something went wrong. Please try again.');
      }
    });
  }

  resetPassword(): void {
    if (this.resetForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const value = this.resetForm.getRawValue();

    this.auth
      .resetPassword({
        usernameOrEmail: this.requestForm.getRawValue().usernameOrEmail,
        code: value.code,
        newPassword: value.newPassword
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/login']);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(err.error?.title ?? 'That code is invalid or has expired.');
        }
      });
  }
}
