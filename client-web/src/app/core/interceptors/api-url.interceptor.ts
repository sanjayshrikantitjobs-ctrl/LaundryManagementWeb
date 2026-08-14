import { HttpInterceptorFn } from '@angular/common/http';
import { API_ORIGIN } from '../config/api-origin';

export const apiUrlInterceptor: HttpInterceptorFn = (req, next) => {
  console.log('INTERCEPTOR', {
    API_ORIGIN,
    original: req.url,
    startsWithApi: req.url.startsWith('/api')
  });

  if (!API_ORIGIN || !req.url.startsWith('/api')) {
    return next(req);
  }

  const url = `${API_ORIGIN}${req.url}`;
  console.log('REWRITTEN', url);

  return next(req.clone({ url }));
};