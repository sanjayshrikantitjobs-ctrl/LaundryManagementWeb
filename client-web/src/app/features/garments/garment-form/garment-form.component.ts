import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GarmentsService } from '../garments.service';

@Component({
  selector: 'app-garment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './garment-form.component.html',
  styleUrl: './garment-form.component.scss'
})
export class GarmentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly garmentsService = inject(GarmentsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private garmentId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    category: ['', [Validators.required, Validators.maxLength(100)]],
    barcode: [''],
    specialInstructions: ['']
  });

  ngOnInit(): void {
    this.garmentId = this.route.snapshot.paramMap.get('id');
    if (this.garmentId) {
      this.isEditMode.set(true);
      this.garmentsService.getGarmentById(this.garmentId).subscribe((garment) => {
        this.form.patchValue({
          name: garment.name,
          category: garment.category,
          barcode: garment.barcode,
          specialInstructions: garment.specialInstructions
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
      category: value.category!,
      barcode: value.barcode || undefined,
      specialInstructions: value.specialInstructions || undefined
    };

    const request$: Observable<unknown> = this.garmentId
      ? this.garmentsService.updateGarment(this.garmentId, request)
      : this.garmentsService.createGarment(request);

    request$.subscribe({
      next: () => this.router.navigate(['/garments']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save garment.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/garments']);
  }
}
