import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GarmentsService } from '../garments.service';
import { UploadService } from '../../../core/services/upload.service';
import { CategoriesService } from '../../categories/categories.service';
import { ServiceCategory } from '../../../core/models/catalog.models';

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
  private readonly uploadService = inject(UploadService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly isUploading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly imageUrl = signal<string | undefined>(undefined);
  readonly categories = signal<ServiceCategory[]>([]);

  private garmentId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    categoryId: ['', [Validators.required]],
    specialInstructions: ['']
  });

  ngOnInit(): void {
    this.categoriesService.getCategories().subscribe((categories) => this.categories.set(categories));

    this.garmentId = this.route.snapshot.paramMap.get('id');
    if (this.garmentId) {
      this.isEditMode.set(true);
      this.garmentsService.getGarmentById(this.garmentId).subscribe((garment) => {
        this.form.patchValue({
          name: garment.name,
          categoryId: garment.categoryId,
          specialInstructions: garment.specialInstructions
        });
        this.imageUrl.set(garment.imageUrl);
      });
    }
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.isUploading.set(true);
    this.errorMessage.set(null);
    this.uploadService.uploadImage(file).subscribe({
      next: (result) => {
        this.imageUrl.set(result.url);
        this.isUploading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to upload image.');
        this.isUploading.set(false);
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
    const request = {
      name: value.name!,
      categoryId: value.categoryId!,
      specialInstructions: value.specialInstructions || undefined,
      imageUrl: this.imageUrl()
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
