/** Where each role lands by default (root path, post-login, and route-guard redirects). */
export function homeRouteForRole(role: string | undefined | null): string {
  switch (role) {
    case 'Customer':
      return '/shop';
    case 'PickupAgent':
      return '/pickup-queue';
    case 'DeliveryAgent':
      return '/delivery-queue';
    default:
      return '/orders';
  }
}
