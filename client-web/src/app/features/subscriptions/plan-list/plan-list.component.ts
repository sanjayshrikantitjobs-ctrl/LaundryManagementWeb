import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SubscriptionsService } from '../subscriptions.service';
import { BillingCycle, SubscriptionPlan } from '../../../core/models/subscription.models';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

interface PlanGroup {
  billingCycle: BillingCycle;
  title: string;
  plans: SubscriptionPlan[];
}

const CYCLE_TITLES: Record<BillingCycle, string> = {
  [BillingCycle.Monthly]: 'Monthly Plans',
  [BillingCycle.Quarterly]: 'Quarterly Plans',
  [BillingCycle.HalfYearly]: 'Half-Yearly Plans',
  [BillingCycle.Yearly]: 'Yearly Plans'
};

@Component({
  selector: 'app-plan-list',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './plan-list.component.html',
  styleUrl: './plan-list.component.scss'
})
export class PlanListComponent implements OnInit {
  private readonly subscriptionsService = inject(SubscriptionsService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly isLoading = signal(true);

  readonly groups = computed<PlanGroup[]>(() => {
    const groups: PlanGroup[] = [];
    for (const plan of this.plans()) {
      let group = groups.find((g) => g.billingCycle === plan.billingCycle);
      if (!group) {
        group = { billingCycle: plan.billingCycle, title: CYCLE_TITLES[plan.billingCycle], plans: [] };
        groups.push(group);
      }
      group.plans.push(plan);
    }
    return groups;
  });

  ngOnInit(): void {
    this.loadPlans();
  }

  async deletePlan(plan: SubscriptionPlan): Promise<void> {
    const result = await this.confirmDialog.confirm({
      title: 'Delete plan',
      message: `Delete plan "${plan.name}"? This cannot be undone.`,
      requireReason: true,
      confirmLabel: 'Delete',
      danger: true
    });
    if (!result.confirmed) return;

    this.subscriptionsService.deletePlan(plan.id, result.reason).subscribe({
      next: () => this.loadPlans(),
      error: (err) => alert(err.error?.title ?? 'Failed to delete plan — customers may still be on it.')
    });
  }

  private loadPlans(): void {
    this.isLoading.set(true);
    this.subscriptionsService.getPlans().subscribe({
      next: (plans) => {
        this.plans.set(plans);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
