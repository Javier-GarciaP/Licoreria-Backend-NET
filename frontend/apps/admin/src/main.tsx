import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { queryClient } from './lib/queryClient';
import { ErrorBoundary } from './components/ErrorBoundary';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { ModoProvider } from './context/ModoContext';
import App from './App';
import './styles/index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <ModoProvider>
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
                    background: 'rgb(var(--color-elevated))',
                    color: 'rgb(var(--color-ink))',
                    border: '1px solid rgb(var(--color-hairline))',
                    borderRadius: '18px',
                  },
                }}
              />
            </AuthProvider>
          </BrowserRouter>
        </QueryClientProvider>
      </ModoProvider>
    </ThemeProvider>
  </StrictMode>,
);
