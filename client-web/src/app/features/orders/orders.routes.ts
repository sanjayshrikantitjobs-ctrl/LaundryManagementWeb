import { Routes } from '@angular/router';
import { CUSTOMER_AND_MANAGEMENT_ROLES, roleGuard } from '../../core/guards/role.guard';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard(CUSTOMER_AND_MANAGEMENT_ROLES)],
    loadComponent: () => import('./order-list/order-list.component').then((m) => m.OrderListComponent)
  },
  {
    path: 'new',
    canActivate: [roleGuard(CUSTOMER_AND_MANAGEMENT_ROLES)],
    loadComponent: () => import('./order-form/order-form.component').then((m) => m.OrderFormComponent)
  },
  {
    // No role guard: Pickup/Delivery Agents, Customers, and management all need
    // to open individual orders (via their queue, "my orders", or the admin list).
    // Ownership/role scoping happens server-side and inside the component itself.
    path: ':id',
    loadComponent: () => import('./order-detail/order-detail.component').then((m) => m.OrderDetailComponent)
  }
];
