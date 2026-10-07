import { useEffect, useMemo, useState } from 'react';
import { Button, Input, Modal, Select } from '@licoreria/ui';
import type { Cuenta, MetodoPago, Moneda } from '@licoreria/types';
import { formatUSD } from '../../lib/format';

export interface AbonoItemSeleccion {
  detalleId: string;
  nombre: string;
  monto: number;
}

export interface AbonoBody {
  metodoPagoId: string;
  monto: number;
  moneda: Moneda;
  items?: { comandaDetalleId: string; monto: number }[];
}

export function AbonoSheet({
  abierto,
  cuenta,
  items,
  metodos,
  registrando,
  onCerrar,
  onConfirmar,
}: {
  abierto: boolean;
  cuenta: Cuenta;
  items?: AbonoItemSeleccion[];
  metodos: MetodoPago[];
  registrando: boolean;
  onCerrar: () => void;
  onConfirmar: (body: AbonoBody) => void;
}) {
  const [metodoPagoId, setMetodoPagoId] = useState('');
  const [moneda, setMoneda] = useState<Moneda>('USD');
  const [monto, setMonto] = useState('');

  const totalItems = useMemo(() => items?.reduce((total, item) => total + item.monto, 0) ?? 0, [items]);
  const porItems = (items?.length ?? 0) > 0;

  useEffect(() => {
    if (!abierto) return;
    setMetodoPagoId(metodos[0]?.id ?? '');
    setMoneda('USD');
    setMonto(porItems ? totalItems.toFixed(2) : '');
  }, [abierto, metodos, porItems, totalItems]);

  const valor = Number(monto);
  const valido = Boolean(metodoPagoId) && Number.isFinite(valor) && valor > 0;

  const confirmar = () => {
    if (!valido) return;
    onConfirmar({
      metodoPagoId,
      monto: valor,
      moneda,
      items: porItems ? items!.map((item) => ({ comandaDetalleId: item.detalleId, monto: item.monto })) : undefined,
    });
  };

  return (
    <Modal
      open={abierto}
      onClose={onCerrar}
      title="Registrar abono"
      className="max-w-md"
      footer={
        <>
          <Button variant="ghost" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button loading={registrando} disabled={!valido} onClick={confirmar}>
            Abonar {formatUSD(porItems ? totalItems : Number.isFinite(valor) ? valor : 0)}
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between rounded-inner bg-elevated/40 px-3 py-2 text-sm">
          <span className="text-muted">Saldo de la cuenta</span>
          <span className="num font-medium text-accent-ink">{formatUSD(cuenta.saldo)}</span>
        </div>

        {porItems ? (
          <div className="rounded-inner border border-hairline p-3">
            <p className="mb-2 text-xs font-medium uppercase tracking-tighter2 text-muted">Consumos a cubrir</p>
            <ul className="flex flex-col gap-1">
              {items!.map((item) => (
                <li key={item.detalleId} className="flex justify-between text-sm text-muted">
                  <span className="truncate">{item.nombre}</span>
                  <span className="num text-ink">{formatUSD(item.monto)}</span>
                </li>
              ))}
            </ul>
          </div>
        ) : (
          <div className="flex flex-wrap gap-2">
            <Button size="sm" variant="ghost" onClick={() => setMonto(cuenta.saldo.toFixed(2))}>
              Saldo total
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setMonto((cuenta.saldo / 2).toFixed(2))}>
              Mitad
            </Button>
          </div>
        )}

        <div className="grid grid-cols-2 gap-3">
          <Select label="Método de pago" value={metodoPagoId} onChange={(evento) => setMetodoPagoId(evento.target.value)}>
            <option value="">Selecciona…</option>
            {metodos.map((metodo) => (
              <option key={metodo.id} value={metodo.id}>
                {metodo.nombre}
              </option>
            ))}
          </Select>
          <Select label="Moneda" value={moneda} onChange={(evento) => setMoneda(evento.target.value as Moneda)}>
            <option value="USD">USD</option>
            <option value="BS">Bs</option>
          </Select>
        </div>

        <Input
          label="Monto"
          type="number"
          inputMode="decimal"
          value={monto}
          readOnly={porItems}
          onChange={(evento) => setMonto(evento.target.value)}
        />
        {moneda === 'BS' && valor > 0 && (
          <p className="text-xs text-muted">Equivalente aproximado en Bs según tasa vigente.</p>
        )}
      </div>
    </Modal>
  );
}
