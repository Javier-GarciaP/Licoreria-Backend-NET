import { useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Plus } from 'lucide-react';
import { BubbleModal, Button, Card, CardBody, CardHeader, CardTitle, DataTable, Input, Pill, Select } from '@licoreria/ui';
import type { TasaCambio } from '@licoreria/types';
import { finanzasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquema = z.object({
  fecha: z.string().optional(),
  tipo: z.enum(['BCV', 'Paralelo']),
  valor: z.coerce.number({ invalid_type_error: 'Ingresa el valor' }).positive('Debe ser mayor que 0'),
});

type Formulario = z.infer<typeof esquema>;

export function TasasPage() {
  const queryClient = useQueryClient();
  const [abierta, setAbierta] = useState(false);
  const botonRef = useRef<HTMLButtonElement>(null);
  const form = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: { fecha: '', tipo: 'Paralelo', valor: 0 } });

  const actual = useQuery({ queryKey: ['tasa-actual'], queryFn: () => finanzasApi.tasaActual('Paralelo') });
  const historico = useQuery({ queryKey: ['tasas'], queryFn: () => finanzasApi.tasas() });

  const registrar = useMutation({
    mutationFn: (datos: Formulario) =>
      finanzasApi.registrarTasa({
        fecha: datos.fecha ? new Date(datos.fecha).toISOString() : null,
        tipo: datos.tipo,
        valor: datos.valor,
      }),
    onSuccess: () => {
      toast.success('Tasa registrada');
      setAbierta(false);
      form.reset({ fecha: '', tipo: 'Paralelo', valor: 0 });
      queryClient.invalidateQueries({ queryKey: ['tasas'] });
      queryClient.invalidateQueries({ queryKey: ['tasa-actual'] });
    },
    onError: (error) => toast.error('No se pudo registrar la tasa', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground">Registra la tasa del día para los cobros en Bs.</p>
        <div className="flex items-center gap-2">
          {actual.data && <Pill tone="accent">Paralelo: {formatUSD(actual.data.valor)}</Pill>}
          <span ref={botonRef}>
            <Button size="sm" leftIcon={<Plus size={15} />} onClick={() => setAbierta(true)}>
              Registrar tasa
            </Button>
          </span>
        </div>
      </div>

      <BubbleModal
        open={abierta}
        onClose={() => setAbierta(false)}
        title="Registrar tasa"
        anchor={botonRef.current}
      >
        <form className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => registrar.mutate(d))} noValidate>
          <Select label="Tipo" error={form.formState.errors.tipo?.message} {...form.register('tipo')}>
            <option value="Paralelo">Paralelo</option>
            <option value="BCV">BCV</option>
          </Select>
          <Input
            label="Valor (Bs por USD)"
            type="number"
            step="0.01"
            error={form.formState.errors.valor?.message}
            {...form.register('valor')}
          />
          <Input label="Fecha (opcional)" type="datetime-local" {...form.register('fecha')} />
          <div className="flex justify-end gap-2">
            <Button variant="ghost" size="sm" onClick={() => setAbierta(false)}>
              Cancelar
            </Button>
            <Button size="sm" type="submit" loading={registrar.isPending}>
              Registrar
            </Button>
          </div>
        </form>
      </BubbleModal>

      <Card>
        <CardHeader>
          <CardTitle>Histórico</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<TasaCambio>
            rows={historico.data ?? []}
            loading={historico.isLoading}
            rowKey={(tasa) => tasa.id}
            empty="Sin tasas registradas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (tasa) => formatDateTime(tasa.fecha) },
              { key: 'tipo', header: 'Tipo', render: (tasa) => <Pill tone={tasa.tipo === 'BCV' ? 'info' : 'accent'}>{tasa.tipo}</Pill> },
              { key: 'valor', header: 'Valor', align: 'right', render: (tasa) => formatUSD(tasa.valor) },
            ]}
          />
        </CardBody>
      </Card>
    </div>
  );
}