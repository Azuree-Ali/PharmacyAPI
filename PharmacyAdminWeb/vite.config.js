import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const target = process.env.PHARMACY_API_URL || 'http://localhost:5114';
export default defineConfig({
  plugins: [react()],
  server: { host: '0.0.0.0', proxy: { '/api': { target, changeOrigin: true, secure: false }, '/hubs': { target, changeOrigin: true, ws: true, secure: false } } },
  preview: { host: '0.0.0.0' }
});
