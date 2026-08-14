import { HttpInterceptorFn } from '@angular/common/http';
import { API_ORIGIN } from '../config/api-origin';

export const apiUrlInterceptor: HttpInterceptorFn = (req, next) => {
  if (!API_ORIGIN || !req.url.startsWith('/api')) return next(req);

  return next(req.clone({ url: `${API_ORIGIN}${req.url}` }));
};
