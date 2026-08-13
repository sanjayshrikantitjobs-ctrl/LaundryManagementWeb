import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { homeRouteForRole } from '../utils/role-home.util';

/** Sends '' and unmatched paths to the right landing page for the logged-in
 * role (Customer -> /shop, PickupAgent -> /pickup-queue, etc.) instead of a
 * single hardcoded redirectTo that only made sense for management roles. */
@Component({
  selector: 'app-home-redirect',
  standalone: true,
  template: ''
})
export class HomeRedirectComponent {
  constructor() {
    const router = inject(Router);
    const auth = inject(AuthService);
    router.navigateByUrl(homeRouteForRole(auth.currentUser()?.role), { replaceUrl: true });
  }
}
