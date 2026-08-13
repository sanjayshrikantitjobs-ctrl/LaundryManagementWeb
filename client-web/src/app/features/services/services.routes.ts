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
  },
  {
    path: 'categories',
    loadComponent: () => import('../categories/category-list/category-list.component').then((m) => m.CategoryListComponent)
  },
  {
    path: 'categories/new',
    loadComponent: () => import('../categories/category-form/category-form.component').then((m) => m.CategoryFormComponent)
  },
  {
    path: 'categories/:id/edit',
    loadComponent: () => import('../categories/category-form/category-form.component').then((m) => m.CategoryFormComponent)
  }
];
