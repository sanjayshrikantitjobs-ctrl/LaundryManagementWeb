import { Routes } from '@angular/router';

export const GARMENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./garment-list/garment-list.component').then((m) => m.GarmentListComponent)
  },
  {
    path: 'pricing',
    loadComponent: () => import('./pricing-matrix/pricing-matrix.component').then((m) => m.PricingMatrixComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./garment-form/garment-form.component').then((m) => m.GarmentFormComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./garment-form/garment-form.component').then((m) => m.GarmentFormComponent)
  }
];
