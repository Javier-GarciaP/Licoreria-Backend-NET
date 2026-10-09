import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { queryClient } from './lib/queryClient';
import { ErrorBoundary } from './components/ErrorBoundary';
import { AuthProvider } from './context/AuthContext';
import { ModoProvider } from './context/ModoContext';
import { ThemeProvider } from './context/ThemeContext';
import App from './App';
import './styles/index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthProvider>
            <ModoProvider>
              <ErrorBoundary>
                <App />
              </ErrorBoundary>
              <Toaster
                position="top-right"
                toastOptions={{
                  style: {
                    background: 'rgb(var(--card))',
                    color: 'rgb(var(--foreground))',
                    border: '1px solid rgb(var(--border))',
                    borderRadius: '12px',
                    boxShadow: 'var(--shadow-card)',
                  },
                }}
              />
            </ModoProvider>
          </AuthProvider>
        </BrowserRouter>
      </QueryClientProvider>
    </ThemeProvider>
  </StrictMode>,
);
