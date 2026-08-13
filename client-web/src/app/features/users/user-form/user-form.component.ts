import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, of, switchMap } from 'rxjs';
import { UsersService } from '../users.service';
import { STAFF_ROLES, UserRole } from '../../../core/models/user.models';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersService = inject(UsersService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly staffRoles = STAFF_ROLES;
  readonly UserRole = UserRole;

  private userId: string | null = null;
  private originalRole: UserRole | null = null;

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    userName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    password: ['', [Validators.minLength(8)]],
    role: [UserRole.Staff, [Validators.required]]
  });

  roleLabel(role: UserRole): string {
    return UserRole[role];
  }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (this.userId) {
      this.isEditMode.set(true);
      this.form.get('userName')?.disable();
      this.form.get('password')?.clearValidators();

      this.usersService.getUserById(this.userId).subscribe((user) => {
        this.originalRole = user.role;
        this.form.patchValue({
          fullName: user.fullName,
          userName: user.userName,
          email: user.email,
          phoneNumber: user.phoneNumber,
          role: user.role
        });
      });
    } else {
      this.form.get('password')?.setValidators([Validators.required, Validators.minLength(8)]);
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

    const roleChanged = this.userId !== null && this.originalRole !== null && value.role !== this.originalRole;

    const request$: Observable<unknown> = this.userId
      ? this.usersService.updateUser(this.userId, {
          fullName: value.fullName!,
          email: value.email || undefined,
          phoneNumber: value.phoneNumber || undefined
        }).pipe(
          switchMap(() => (roleChanged ? this.usersService.assignRole(this.userId!, value.role!) : of(undefined)))
        )
      : this.usersService.createUser({
          fullName: value.fullName!,
          userName: value.userName!,
          email: value.email || undefined,
          phoneNumber: value.phoneNumber || undefined,
          password: value.password!,
          role: value.role!
        });

    request$.subscribe({
      next: () => this.router.navigate(['/users']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save user.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}
