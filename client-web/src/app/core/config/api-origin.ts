const AZURE_API_ORIGIN = 'https://laundrymgmt-api-hmesarcqhtchg8gg.centralindia-01.azurewebsites.net';

const isLocalDev = location.hostname === 'localhost' || location.hostname === '127.0.0.1';

export const API_ORIGIN = isLocalDev ? '' : AZURE_API_ORIGIN;
