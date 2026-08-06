import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ServicesService } from '../services.service';

@Component({
  selector: 'app-service-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './service-form.component.html',
  styleUrl: './service-form.component.scss'
})
export class ServiceFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly servicesService = inject(ServicesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private serviceId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    basePrice: [0, [Validators.required, Validators.min(0)]],
    estimatedTimeHours: [1, [Validators.required, Validators.min(1)]],
    gstPercentage: [5, [Validators.required, Validators.min(0), Validators.max(100)]],
    priority: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.serviceId = this.route.snapshot.paramMap.get('id');
    if (this.serviceId) {
      this.isEditMode.set(true);
      this.servicesService.getServiceById(this.serviceId).subscribe((service) => {
        this.form.patchValue({
          name: service.name,
          basePrice: service.basePrice,
          estimatedTimeHours: service.estimatedTimeHours,
          gstPercentage: service.gstPercentage,
          priority: service.priority
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
    const request = {
      name: value.name!,
      basePrice: value.basePrice!,
      estimatedTimeHours: value.estimatedTimeHours!,
      gstPercentage: value.gstPercentage!,
      priority: value.priority!
    };

    const request$: Observable<unknown> = this.serviceId
      ? this.servicesService.updateService(this.serviceId, request)
      : this.servicesService.createService(request);

    request$.subscribe({
      next: () => this.router.navigate(['/services']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save service.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/services']);
  }
}
