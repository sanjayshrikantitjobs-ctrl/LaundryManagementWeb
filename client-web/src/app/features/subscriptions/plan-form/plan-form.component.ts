import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubscriptionsService } from '../subscriptions.service';
import { BillingCycle } from '../../../core/models/subscription.models';

@Component({
  selector: 'app-plan-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './plan-form.component.html',
  styleUrl: './plan-form.component.scss'
})
export class PlanFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly isEditMode = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly BillingCycle = BillingCycle;

  private planId: string | null = null;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    billingCycle: [BillingCycle.Monthly],
    garmentsPerCycle: [20, [Validators.required, Validators.min(0)]],
    price: [0, [Validators.required, Validators.min(0)]],
    displayOrder: [0, [Validators.required, Validators.min(0)]],
    isActive: [true],
    features: this.fb.array([this.buildFeatureControl('')])
  });

  get features(): FormArray {
    return this.form.get('features') as FormArray;
  }

  ngOnInit(): void {
    this.planId = this.route.snapshot.paramMap.get('id');
    if (this.planId) {
      this.isEditMode.set(true);
      this.subscriptionsService.getPlans().subscribe((plans) => {
        const plan = plans.find((p) => p.id === this.planId);
        if (!plan) return;

        this.form.patchValue({
          name: plan.name,
          description: plan.description,
          billingCycle: plan.billingCycle,
          garmentsPerCycle: plan.garmentsPerCycle,
          price: plan.price,
          displayOrder: plan.displayOrder,
          isActive: plan.isActive
        });

        this.features.clear();
        for (const feature of plan.features) this.features.push(this.buildFeatureControl(feature.text));
        if (this.features.length === 0) this.features.push(this.buildFeatureControl(''));
      });
    }
  }

  addFeature(): void {
    this.features.push(this.buildFeatureControl(''));
  }

  removeFeature(index: number): void {
    if (this.features.length > 1) this.features.removeAt(index);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const features = (value.features ?? []).map((f) => (f ?? '').trim()).filter((f) => f.length > 0);
    const request = {
      name: value.name!,
      description: value.description || undefined,
      billingCycle: value.billingCycle!,
      garmentsPerCycle: value.garmentsPerCycle!,
      price: value.price!,
      displayOrder: value.displayOrder!,
      isActive: value.isActive!,
      features
    };

    const request$: Observable<unknown> = this.planId
      ? this.subscriptionsService.updatePlan(this.planId, request)
      : this.subscriptionsService.createPlan(request);

    request$.subscribe({
      next: () => this.router.navigate(['/subscriptions']),
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(err.error?.title ?? 'Failed to save plan.');
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/subscriptions']);
  }

  private buildFeatureControl(text: string) {
    return this.fb.control(text);
  }
}
