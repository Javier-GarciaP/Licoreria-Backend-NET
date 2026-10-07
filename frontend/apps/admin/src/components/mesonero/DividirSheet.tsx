import { useEffect, useState } from 'react';
import { Plus, Trash2 } from 'lucide-react';
import { Button, Input, Modal, cn } from '@licoreria/ui';
import type { Cuenta } from '@licoreria/types';
import { formatUSD } from '../../lib/format';

export function DividirSheet({
  abierto,
  cuenta,
  registrando,
  onCerrar,
  onConfirmar,
}: {
  abierto: boolean;
  cuenta: Cuenta;
  registrando: boolean;
  onCerrar: () => void;
  onConfirmar: (dto: { partes: number } | { montos: number[] }) => void;
}) {
  const [modo, setModo] = useState<'partes' | 'montos'>('partes');
  const [partes, setPartes] = useState(2);
  const [montos, setMontos] = useState<string[]>(['', '']);

  useEffect(() => {
    if (!abierto) return;
    setModo('partes');
    setPartes(2);
    setMontos([(cuenta.saldo / 2).toFixed(2), (cuenta.saldo / 2).toFixed(2)]);
  }, [abierto, cuenta.saldo]);

  const sumaMontos = montos.reduce((total, valor) => total + (Number(valor) || 0), 0);
  const montosValidos =
    montos.length >= 2 && montos.every((valor) => Number(valor) > 0) && Math.abs(sumaMontos - cuenta.saldo) <= 0.01;

  const puede = modo === 'partes' ? partes >= 2 : montosValidos;

  const confirmar = () => {
    if (!puede) return;
    onConfirmar(modo === 'partes' ? { partes } : { montos: montos.map((valor) => Number(valor)) });
  };

  return (
    <Modal
      open={abierto}
      onClose={onCerrar}
      title="Dividir cuenta"
      className="max-w-md"
      footer={
        <>
          <Button variant="ghost" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button loading={registrando} disabled={!puede} onClick={confirmar}>
            Dividir
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between rounded-inner bg-elevated/40 px-3 py-2 text-sm">
          <span className="text-muted">Saldo a dividir</span>
          <span className="num font-medium text-accent-ink">{formatUSD(cuenta.saldo)}</span>
        </div>

        <div className="flex gap-1">
          {(['partes', 'montos'] as const).map((valor) => (
            <button
              key={valor}
              type="button"
              onClick={() => setModo(valor)}
              className={cn(
                'flex-1 rounded-pill px-3 py-2 text-sm capitalize transition',
                modo === valor ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:text-ink',
              )}
            >
              {valor === 'partes' ? 'Partes iguales' : 'Montos'}
            </button>
          ))}
        </div>

        {modo === 'partes' ? (
          <div className="flex flex-col gap-3">
            <div className="flex items-center justify-center gap-4">
              <Button
                variant="ghost"
                size="icon"
                aria-label="Menos partes"
                onClick={() => setPartes((valor) => Math.max(2, valor - 1))}
              >
                −
              </Button>
              <span className="num text-2xl font-medium text-ink">{partes}</span>
              <Button
                variant="ghost"
                size="icon"
                aria-label="Más partes"
                onClick={() => setPartes((valor) => Math.min(20, valor + 1))}
              >
                <Plus size={16} />
              </Button>
            </div>
            <p className="text-center text-sm text-muted">
              {partes} partes de <span className="num text-ink">{formatUSD(cuenta.saldo / partes)}</span>
            </p>
          </div>
        ) : (
          <div className="flex flex-col gap-2">
            {montos.map((valor, indice) => (
              <div key={indice} className="flex items-end gap-2">
                <Input
                  label={`Parte ${indice + 1}`}
                  type="number"
                  inputMode="decimal"
                  value={valor}
                  onChange={(evento) =>
                    setMontos((actuales) => actuales.map((m, i) => (i === indice ? evento.target.value : m)))
                  }
                />
                {montos.length > 2 && (
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Quitar parte ${indice + 1}`}
                    onClick={() => setMontos((actuales) => actuales.filter((_, i) => i !== indice))}
                  >
                    <Trash2 size={15} />
                  </Button>
                )}
              </div>
            ))}
            <Button variant="ghost" size="sm" leftIcon={<Plus size={15} />} onClick={() => setMontos((actuales) => [...actuales, ''])}>
              Añadir parte
            </Button>
            <div className="flex justify-between text-sm">
              <span className="text-muted">Suma</span>
              <span className={cn('num', montosValidos ? 'text-success-ink' : 'text-danger-ink')}>{formatUSD(sumaMontos)}</span>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
}
