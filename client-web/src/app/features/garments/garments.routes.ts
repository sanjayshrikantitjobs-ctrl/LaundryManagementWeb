import { Routes } from '@angular/router';
import { CUSTOMER_AND_MANAGEMENT_ROLES, MANAGEMENT_ROLES, roleGuard } from '../../core/guards/role.guard';

export const GARMENTS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./garment-list/garment-list.component').then((m) => m.GarmentListComponent)
  },
  {
    path: 'pricing',
    canActivate: [roleGuard(CUSTOMER_AND_MANAGEMENT_ROLES)],
    loadComponent: () => import('./pricing-matrix/pricing-matrix.component').then((m) => m.PricingMatrixComponent)
  },
  {
    path: 'new',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./garment-form/garment-form.component').then((m) => m.GarmentFormComponent)
  },
  {
    path: ':id/edit',
    canActivate: [roleGuard(MANAGEMENT_ROLES)],
    loadComponent: () => import('./garment-form/garment-form.component').then((m) => m.GarmentFormComponent)
  }
];
