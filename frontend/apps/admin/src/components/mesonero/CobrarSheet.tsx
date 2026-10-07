import { useEffect, useState } from 'react';
import { Button, Input, Modal, Select } from '@licoreria/ui';
import type { Cuenta, MetodoPago } from '@licoreria/types';
import { formatUSD } from '../../lib/format';

export interface CobrarBody {
  descuentoUSD: number;
  pagos: { metodoPagoId: string; monto: number; moneda: 'USD' | 'BS' }[];
}

export function CobrarSheet({
  abierto,
  cuenta,
  metodos,
  registrando,
  onCerrar,
  onConfirmar,
}: {
  abierto: boolean;
  cuenta: Cuenta;
  metodos: MetodoPago[];
  registrando: boolean;
  onCerrar: () => void;
  onConfirmar: (body: CobrarBody) => void;
}) {
  const [metodoPagoId, setMetodoPagoId] = useState('');
  const [monto, setMonto] = useState('');
  const [descuento, setDescuento] = useState('');

  const saldo = Math.max(0, cuenta.saldo);
  const descuentoValor = Math.max(0, Number(descuento) || 0);

  useEffect(() => {
    if (!abierto) return;
    setMetodoPagoId(metodos[0]?.id ?? '');
    setMonto(saldo.toFixed(2));
    setDescuento('');
  }, [abierto, metodos, saldo]);

  const requierePago = saldo > 0;
  const valor = Number(monto);
  const valido = !requierePago || (Boolean(metodoPagoId) && Number.isFinite(valor) && valor > 0);

  const confirmar = () => {
    if (!valido) return;
    onConfirmar({
      descuentoUSD: descuentoValor,
      pagos: requierePago ? [{ metodoPagoId, monto: valor, moneda: 'USD' }] : [],
    });
  };

  return (
    <Modal
      open={abierto}
      onClose={onCerrar}
      title="Cobrar y cerrar"
      className="max-w-md"
      footer={
        <>
          <Button variant="ghost" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button loading={registrando} disabled={!valido} onClick={confirmar}>
            Cobrar {formatUSD(Math.max(0, saldo - descuentoValor))}
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-4">
        <div className="grid grid-cols-3 gap-2 text-center">
          {[
            ['Total', cuenta.total],
            ['Abonado', cuenta.totalAbonado],
            ['Saldo', saldo],
          ].map(([etiqueta, valor]) => (
            <div key={etiqueta as string} className="rounded-inner bg-elevated/40 px-2 py-2">
              <p className="text-[10px] uppercase tracking-tighter2 text-muted">{etiqueta}</p>
              <p className="num text-sm font-medium text-ink">{formatUSD(valor as number)}</p>
            </div>
          ))}
        </div>

        {!requierePago ? (
          <p className="rounded-inner border border-success/40 bg-success/10 px-3 py-2 text-sm text-success-ink">
            Cubierto por abonos. Solo falta cerrar la cuenta.
          </p>
        ) : (
          <>
            <Input
              label="Descuento (USD)"
              type="number"
              inputMode="decimal"
              value={descuento}
              onChange={(evento) => setDescuento(evento.target.value)}
            />
            <div className="grid grid-cols-2 gap-3">
              <Select label="Método de pago" value={metodoPagoId} onChange={(evento) => setMetodoPagoId(evento.target.value)}>
                <option value="">Selecciona…</option>
                {metodos.map((metodo) => (
                  <option key={metodo.id} value={metodo.id}>
                    {metodo.nombre}
                  </option>
                ))}
              </Select>
              <Input label="Monto USD" type="number" inputMode="decimal" value={monto} onChange={(evento) => setMonto(evento.target.value)} />
            </div>
          </>
        )}
      </div>
    </Modal>
  );
}
