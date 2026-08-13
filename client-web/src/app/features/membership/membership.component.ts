import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SubscriptionsService } from '../subscriptions/subscriptions.service';
import { BillingCycle, SubscriptionPlan } from '../../core/models/subscription.models';

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
  selector: 'app-membership',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './membership.component.html',
  styleUrl: './membership.component.scss'
})
export class MembershipComponent implements OnInit {
  private readonly subscriptionsService = inject(SubscriptionsService);

  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly isLoading = signal(true);

  readonly groups = computed<PlanGroup[]>(() => {
    const groups: PlanGroup[] = [];
    for (const plan of this.plans()) {
      if (!plan.isActive) continue;
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
    this.subscriptionsService.getPlans().subscribe({
      next: (plans) => {
        this.plans.set(plans);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}
