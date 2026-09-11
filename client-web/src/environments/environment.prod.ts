export const environment = {
  production: true,
  // Cloudflare Pages can't proxy an external origin (and can't proxy WebSockets at
  // all for the SignalR hub), so the production build calls the API directly. The
  // API's CORS policy already allows this Pages origin.
  apiBaseUrl: 'https://laundrymgmt-api-hmesarcqhtchg8gg.centralindia-01.azurewebsites.net'
};
