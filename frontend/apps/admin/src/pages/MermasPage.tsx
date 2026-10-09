import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Plus } from 'lucide-react';
import {
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  Input,
  LimpiarFiltros,
  Modal,
  Pagination,
  Select,
} from '@licoreria/ui';
import type { Merma } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const MOTIVOS = ['Danado', 'Partido', 'Vencido'] as const;

const esquema = z.object({
  varianteId: z.string().min(1, 'Selecciona un producto'),
  cantidad: z.coerce.number({ invalid_type_error: 'Ingresa una cantidad' }).positive('Debe ser mayor que 0'),
  motivo: z.enum(MOTIVOS),
  reponerSinCobro: z.boolean(),
});

type Formulario = z.infer<typeof esquema>;

export function MermasPage() {
  const [page, setPage] = useState(1);
  const [registrando, setRegistrando] = useState(false);
  const [busqueda, setBusqueda] = useState('');
  const [motivo, setMotivo] = useState('');
  const [cortesia, setCortesia] = useState('');
  const queryClient = useQueryClient();
  const stock = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock() });

  const hayFiltroLocal = Boolean(busqueda || motivo || cortesia);
  const hayFiltros = hayFiltroLocal;

  const limpiarFiltros = () => {
    setBusqueda('');
    setMotivo('');
    setCortesia('');
    setPage(1);
  };

  const mermas = useQuery({
    queryKey: ['mermas', page, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 10],
    queryFn: () => inventarioApi.mermas({ page: hayFiltroLocal ? 1 : page, pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 10 }),
  });

  const { items: filas, totalPages } = useMemo(() => {
    const filtradas = (mermas.data?.items ?? []).filter((merma) => {
      if (!contiene(`${merma.sku} ${merma.varianteId}`, busqueda)) return false;
      if (motivo && merma.motivo !== motivo) return false;
      if (cortesia && (cortesia === 'si') !== merma.repuesto) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 10)
      : { items: filtradas, totalPages: mermas.data?.totalPages ?? 1 };
  }, [mermas.data, busqueda, motivo, cortesia, hayFiltroLocal, page]);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({
    resolver: zodResolver(esquema),
    defaultValues: { varianteId: '', cantidad: 1, motivo: 'Danado', reponerSinCobro: false },
  });

  const registrar = useMutation({
    mutationFn: (datos: Formulario) => inventarioApi.registrarMerma(datos),
    onSuccess: () => {
      toast.success('Merma registrada');
      setRegistrando(false);
      reset({ varianteId: '', cantidad: 1, motivo: 'Danado', reponerSinCobro: false });
      queryClient.invalidateQueries({ queryKey: ['mermas'] });
      queryClient.invalidateQueries({ queryKey: ['stock'] });
      queryClient.invalidateQueries({ queryKey: ['kardex'] });
      queryClient.invalidateQueries({ queryKey: ['productos'] });
    },
    onError: (error) => toast.error('No se pudo registrar la merma', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar producto o SKU…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <Button leftIcon={<Plus size={15} />} onClick={() => setRegistrando(true)}>
                Nueva merma
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Motivo"
              opciones={MOTIVOS.map((valor) => ({ valor, etiqueta: valor }))}
              valor={motivo}
              onChange={(valor) => {
                setMotivo(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Cortesía"
              opciones={[
                { valor: 'si', etiqueta: 'Sí' },
                { valor: 'no', etiqueta: 'No' },
              ]}
              valor={cortesia}
              onChange={(valor) => {
                setCortesia(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Merma>
            rows={filas}
            loading={mermas.isLoading}
            rowKey={(merma) => merma.id}
            empty="Sin mermas registradas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (merma) => formatDateTime(merma.fecha) },
              { key: 'sku', header: 'SKU', render: (merma) => merma.sku },
              { key: 'cantidad', header: 'Cantidad', align: 'right', render: (merma) => formatNumber(merma.cantidad) },
              { key: 'motivo', header: 'Motivo', render: (merma) => merma.motivo },
              {
                key: 'repuesto',
                header: 'Cortesía',
                render: (merma) => (merma.repuesto ? 'Sí' : 'No'),
              },
            ]}
          />
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={registrando}
        onClose={() => setRegistrando(false)}
        title="Nueva merma"
        footer={
          <>
            <Button variant="ghost" onClick={() => setRegistrando(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-merma" loading={isSubmitting || registrar.isPending}>
              Registrar merma
            </Button>
          </>
        }
      >
        <form
          id="form-merma"
          className="flex flex-col gap-4"
          onSubmit={handleSubmit((datos) => registrar.mutate(datos))}
          noValidate
        >
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Select label="Producto / variante" error={errors.varianteId?.message} {...register('varianteId')}>
              <option value="">Selecciona…</option>
              {stock.data?.items.map((item) => (
                <option key={item.varianteId} value={item.varianteId}>
                  {item.productoNombre} · {item.sku} ({item.cantidad} disp.)
                </option>
              ))}
            </Select>

            <Select label="Motivo" error={errors.motivo?.message} {...register('motivo')}>
              {MOTIVOS.map((valor) => (
                <option key={valor} value={valor}>
                  {valor}
                </option>
              ))}
            </Select>
          </div>

          <Input
            label="Cantidad"
            type="number"
            error={errors.cantidad?.message}
            {...register('cantidad')}
          />

          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            <input type="checkbox" {...register('reponerSinCobro')} />
            Reponer sin cobro (cortesía)
          </label>
        </form>
      </Modal>
    </div>
  );
}