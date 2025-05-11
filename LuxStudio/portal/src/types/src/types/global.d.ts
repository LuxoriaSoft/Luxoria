export {}; // Important for module augmentation

declare global {
  interface Window {
    appConfig: {
      API_URL: string;
      // Add other config properties as needed
    };
  }
}