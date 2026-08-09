import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PromotionsService } from '../../promotions/promotions.service';
import { UploadService } from '../../../core/services/upload.service';

@Component({
  selector: 'app-promotion-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './promotion-form.component.html',
  styleUrl: './promotion-form.component.scss'
})
export class PromotionFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly promotionsService = inject(PromotionsService);
  private readonly uploadService = inject(UploadService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly isUploading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly imageUrl = signal<string | undefined>(undefined);

  private promotionId: string | null = null;

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(150)]],
    description: [''],
    code: [''],
    discountPercent: [null as number | null],
    discountAmount: [null as number | null],
    validFrom: [''],
    validTo: [''],
    isActive: [true]
  });

  ngOnInit(): void {
    this.promotionId = this.route.snapshot.paramMap.get('id');
    if (this.promotionId) {
      this.isEditMode.set(true);
      this.promotionsService.getPromotions({ pageSize: 200 }).subscribe((result) => {
        const promo = result.items.find((p) => p.id === this.promotionId);
        if (!promo) return;
        this.form.patchValue({
          title: promo.title,
          description: promo.description ?? '',
          code: promo.code ?? '',
          discountPercent: promo.discountPercent ?? null,
          discountAmount: promo.discountAmount ?? null,
          validFrom: promo.validFrom ? promo.validFrom.substring(0, 10) : '',
          validTo: promo.validTo ? promo.validTo.substring(0, 10) : '',
          isActive: promo.isActive
        });
        this.imageUrl.set(promo.imageUrl);
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
      title: value.title!,
      description: value.description || undefined,
      imageUrl: this.imageUrl(),
      code: value.code || undefined,
      discountPercent: value.discountPercent,
      discountAmount: value.discountAmount,
      validFrom: value.validFrom ? new Date(value.validFrom).toISOString() : null,
      validTo: value.validTo ? new Date(value.validTo).toISOString() : null,
      isActive: value.isActive!
    };

    const request$: Observable<unknown> = this.promotionId
      ? this.promotionsService.updatePromotion(this.promotionId, request)
      : this.promotionsService.createPromotion(request);

    request$.subscribe({
      next: () => this.router.navigate(['/admin/promotions']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save promotion.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/admin/promotions']);
  }
}
