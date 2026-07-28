import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    usernameOrEmail: ['', Validators.required],
    password: ['', Validators.required]
  });

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/orders']);
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('Invalid username or password.');
      }
    });
  }
}