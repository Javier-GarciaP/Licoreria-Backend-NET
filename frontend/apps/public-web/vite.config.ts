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
  server: { port: 5174, fs: { allow: [resolvePath('../..')] } },
  preview: { port: 4174 },
});
