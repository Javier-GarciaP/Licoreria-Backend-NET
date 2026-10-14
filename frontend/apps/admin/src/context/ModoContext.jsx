import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

const ModoContext = createContext(null);
const STORAGE_KEY = 'licoreria.modo';

/**
 * Modo de operación dual:
 * - "licoreria": venta rápida de mostrador (POS).
 * - "discoteca": gestión de mesas, plano, cover y KDS.
 */
export function ModoProvider({ children }) {
  const [modo, setModoState] = useState(() => localStorage.getItem(STORAGE_KEY) || 'discoteca');

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, modo);
  }, [modo]);

  const setModo = useCallback((nuevo) => setModoState(nuevo), []);
  const alternarModo = useCallback(
    () => setModoState((actual) => (actual === 'discoteca' ? 'licoreria' : 'discoteca')),
    [],
  );

  const value = useMemo(
    () => ({ modo, setModo, alternarModo, esDiscoteca: modo === 'discoteca' }),
    [modo, setModo, alternarModo],
  );

  return <ModoContext.Provider value={value}>{children}</ModoContext.Provider>;
}

export function useModo() {
  const contexto = useContext(ModoContext);
  if (!contexto) throw new Error('useModo debe usarse dentro de ModoProvider');
  return contexto;
}
