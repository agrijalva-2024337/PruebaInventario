export const env = {
  apiUrl: import.meta.env.VITE_API_URL ?? '',
  useApiMock: import.meta.env.VITE_USE_API_MOCK === 'true',
  googleMapsApiKey: import.meta.env.VITE_GOOGLE_MAPS_API_KEY ?? '',
};
