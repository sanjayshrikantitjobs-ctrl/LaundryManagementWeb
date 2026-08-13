import { Routes } from '@angular/router';

export const SUBSCRIPTIONS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./plan-list/plan-list.component').then((m) => m.PlanListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./plan-form/plan-form.component').then((m) => m.PlanFormComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./plan-form/plan-form.component').then((m) => m.PlanFormComponent)
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./customer-subscription-list/customer-subscription-list.component').then(
        (m) => m.CustomerSubscriptionListComponent
      )
  },
  {
    path: 'customers/new',
    loadComponent: () =>
      import('./customer-subscription-form/customer-subscription-form.component').then(
        (m) => m.CustomerSubscriptionFormComponent
      )
  },
  {
    path: 'customers/:id/edit',
    loadComponent: () =>
      import('./customer-subscription-form/customer-subscription-form.component').then(
        (m) => m.CustomerSubscriptionFormComponent
      )
  }
];
