import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

const ThemeContext = createContext(null);
const STORAGE_KEY = 'licoreria.tema';

/**
 * Selector de tema visual persistente: "Oscuro" (por defecto) y "Azul UNET".
 * Aplica data-theme en <html> para que los tokens CSS cambien en caliente.
 */
export function ThemeProvider({ children }) {
  const [tema, setTemaState] = useState(() => localStorage.getItem(STORAGE_KEY) || 'dark');

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', tema);
    localStorage.setItem(STORAGE_KEY, tema);
  }, [tema]);

  const setTema = useCallback((nuevo) => setTemaState(nuevo), []);
  const alternarTema = useCallback(
    () => setTemaState((actual) => (actual === 'dark' ? 'unet' : 'dark')),
    [],
  );

  const value = useMemo(
    () => ({ tema, setTema, alternarTema, esOscuro: tema === 'dark' }),
    [tema, setTema, alternarTema],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const contexto = useContext(ThemeContext);
  if (!contexto) throw new Error('useTheme debe usarse dentro de ThemeProvider');
  return contexto;
}
