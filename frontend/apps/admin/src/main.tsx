import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { queryClient } from './lib/queryClient';
import { ErrorBoundary } from './components/ErrorBoundary';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import App from './App';
import './styles/index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthProvider>
            <ErrorBoundary>
              <App />
            </ErrorBoundary>
            <Toaster
              position="top-right"
              toastOptions={{
                style: {
                  background: 'rgb(var(--color-surface) / 0.88)',
                  color: 'rgb(var(--color-ink))',
                  border: '1px solid rgb(var(--color-ink) / 0.06)',
                  backdropFilter: 'blur(16px) saturate(150%)',
                  WebkitBackdropFilter: 'blur(16px) saturate(150%)',
                  borderRadius: '16px',
                },
              }}
            />
          </AuthProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  </StrictMode>,
);
