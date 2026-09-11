import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/// Prefixes relative /api/... requests with environment.apiBaseUrl (empty in dev,
/// where proxy.conf.json handles it; the deployed Azure API origin in production,
/// since Cloudflare Pages can't proxy to an external origin).
export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  if (!environment.apiBaseUrl || !req.url.startsWith('/api')) return next(req);

  return next(req.clone({ url: environment.apiBaseUrl + req.url }));
};
