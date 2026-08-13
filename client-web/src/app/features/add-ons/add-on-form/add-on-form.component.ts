import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddOnsService } from '../add-ons.service';

@Component({
  selector: 'app-add-on-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-on-form.component.html',
  styleUrl: './add-on-form.component.scss'
})
export class AddOnFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly addOnsService = inject(AddOnsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private addOnId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    price: [0, [Validators.required, Validators.min(0)]],
    isActive: [true]
  });

  ngOnInit(): void {
    this.addOnId = this.route.snapshot.paramMap.get('id');
    if (this.addOnId) {
      this.isEditMode.set(true);
      // No GetById endpoint — mirrors CategoryFormComponent's approach; the add-on
      // list is small and always fully loaded, so finding by id client-side is
      // simpler than adding a dedicated endpoint just for this form.
      this.addOnsService.getAddOns().subscribe((addOns) => {
        const addOn = addOns.find((a) => a.id === this.addOnId);
        if (!addOn) return;

        this.form.patchValue({
          name: addOn.name,
          description: addOn.description,
          price: addOn.price,
          isActive: addOn.isActive
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
      description: value.description || undefined,
      price: value.price!,
      isActive: value.isActive!
    };

    const request$: Observable<unknown> = this.addOnId
      ? this.addOnsService.updateAddOn(this.addOnId, request)
      : this.addOnsService.createAddOn(request);

    request$.subscribe({
      next: () => this.router.navigate(['/add-ons']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save add-on.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/add-ons']);
  }
}
