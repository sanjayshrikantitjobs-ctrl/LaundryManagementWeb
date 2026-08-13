import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoriesService } from '../categories.service';
import { UploadService } from '../../../core/services/upload.service';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss'
})
export class CategoryFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly categoriesService = inject(CategoriesService);
  private readonly uploadService = inject(UploadService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly isUploading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly iconOrImageUrl = signal<string | undefined>(undefined);

  private categoryId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    displayOrder: [0, [Validators.required, Validators.min(0)]],
    isActive: [true]
  });

  ngOnInit(): void {
    this.categoryId = this.route.snapshot.paramMap.get('id');
    if (this.categoryId) {
      this.isEditMode.set(true);
      // No GetById endpoint — the category list is small (~12 rows) and always
      // fully loaded elsewhere, so finding by id client-side is simpler than adding
      // a dedicated endpoint just for this form.
      this.categoriesService.getCategories().subscribe((categories) => {
        const category = categories.find((c) => c.id === this.categoryId);
        if (!category) return;

        this.form.patchValue({
          name: category.name,
          description: category.description,
          displayOrder: category.displayOrder,
          isActive: category.isActive
        });
        this.iconOrImageUrl.set(category.iconOrImageUrl);
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
        this.iconOrImageUrl.set(result.url);
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
      description: value.description || undefined,
      iconOrImageUrl: this.iconOrImageUrl(),
      displayOrder: value.displayOrder!,
      isActive: value.isActive!
    };

    const request$: Observable<unknown> = this.categoryId
      ? this.categoriesService.updateCategory(this.categoryId, request)
      : this.categoriesService.createCategory(request);

    request$.subscribe({
      next: () => this.router.navigate(['/categories']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save category.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/services/categories']);
  }
}
