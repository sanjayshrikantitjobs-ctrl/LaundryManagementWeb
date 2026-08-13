import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { homeRouteForRole } from '../utils/role-home.util';

/** Restricts a route to the given roles; anyone else is sent to their own
 * home route (never just blocked with a blank page). Authentication itself
 * is still enforced separately by authGuard. */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const role = auth.currentUser()?.role;

    if (role && allowedRoles.includes(role)) return true;

    router.navigate([homeRouteForRole(role)]);
    return false;
  };
};

export const MANAGEMENT_ROLES = ['Admin', 'StoreManager', 'Staff', 'DepartmentHead'];
export const CUSTOMER_AND_MANAGEMENT_ROLES = ['Customer', ...MANAGEMENT_ROLES];
export const CUSTOMER_ONLY_ROLES = ['Customer'];
export const ADMIN_ONLY_ROLES = ['Admin'];
export const PICKUP_AGENT_ROLES = ['PickupAgent'];
export const DELIVERY_AGENT_ROLES = ['DeliveryAgent'];
