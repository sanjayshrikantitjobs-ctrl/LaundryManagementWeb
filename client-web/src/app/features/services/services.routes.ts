import { Routes } from '@angular/router';

export const SERVICES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./service-list/service-list.component').then((m) => m.ServiceListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./service-form/service-form.component').then((m) => m.ServiceFormComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./service-form/service-form.component').then((m) => m.ServiceFormComponent)
  }
];
