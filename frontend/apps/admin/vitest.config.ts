import { defineConfig } from 'vitest/config';
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
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    css: false,
  },
});
