import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './verify-otp.component.html',
  styleUrl: './verify-otp.component.scss'
})
export class VerifyOtpComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly phoneNumber = signal(this.route.snapshot.queryParamMap.get('phoneNumber') ?? '');
  readonly devOtpCode = signal(this.route.snapshot.queryParamMap.get('devOtpCode') ?? '');
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    code: [this.devOtpCode(), [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  submit(): void {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.verifyOtp({ phoneNumber: this.phoneNumber(), code: this.form.getRawValue().code }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/shop']);
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('That code is invalid or has expired.');
      }
    });
  }
}
