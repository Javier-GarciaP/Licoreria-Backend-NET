import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { fileURLToPath, URL } from 'node:url';

const resolvePath = (relative: string) => fileURLToPath(new URL(relative, import.meta.url));

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@licoreria/types': resolvePath('../../packages/types/src/index.ts'),
      '@licoreria/api-client': resolvePath('../../packages/api-client/src/index.ts'),
      '@licoreria/ui': resolvePath('../../packages/ui/src/index.ts'),
    },
  },
  server: {
    port: 5175,
    fs: { allow: [resolvePath('../..')] },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          react: ['react', 'react-dom', 'react-router-dom'],
          query: ['@tanstack/react-query'],
          signalr: ['@microsoft/signalr'],
        },
      },
    },
  },
  preview: {
    port: 4175,
  },
});
