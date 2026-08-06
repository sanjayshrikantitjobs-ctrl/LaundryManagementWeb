import { Routes } from '@angular/router';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./order-list/order-list.component').then((m) => m.OrderListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./order-form/order-form.component').then((m) => m.OrderFormComponent)
  }
];
