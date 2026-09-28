import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import mdx from '@astrojs/mdx';
import sitemap from '@astrojs/sitemap';
import starlightOpenAPI, { openAPISidebarGroups } from 'starlight-openapi';
import mermaid from 'astro-mermaid';

export default defineConfig({
  site: 'https://javier-garciap.github.io',
  base: '/Licoreria-Backend-NET/',
  integrations: [
    starlight({
      title: 'Licorería / Discoteca',
      description:
        'Documentación técnica y manuales del sistema de gestión para licorería y discoteca.',
      social: [
        {
          icon: 'github',
          label: 'GitHub',
          href: 'https://github.com/Javier-GarciaP/Licoreria-Backend-NET',
        },
      ],
      defaultLocale: 'root',
      locales: {
        root: { label: 'Español', lang: 'es' },
      },
      plugins: [
        starlightOpenAPI([
          {
            base: 'dev/api',
            schema: '../../openapi/licoreria.yaml',
            label: 'Referencia API',
          },
        ]),
      ],
      sidebar: [
        { label: 'Inicio', items: ['index'] },
        {
          label: 'Desarrolladores',
          items: [
            'dev',
            {
              label: 'Visión',
              items: [
                'dev/00-vision/vision',
                'dev/00-vision/alcance',
                'dev/00-vision/glosario',
              ],
            },
            {
              label: 'Requerimientos',
              items: [
                'dev/01-requerimientos/funcionales',
                'dev/01-requerimientos/no-funcionales',
                'dev/01-requerimientos/historias-usuario',
              ],
            },
            {
              label: 'Arquitectura',
              items: [
                'dev/02-arquitectura/onion',
                'dev/02-arquitectura/c4',
                'dev/02-arquitectura/patrones',
              ],
            },
            {
              label: 'Base de datos',
              items: [
                'dev/03-base-datos/modelo-er',
                'dev/03-base-datos/esquemas',
                'dev/03-base-datos/diccionario-datos',
                'dev/03-base-datos/postgresql',
              ],
            },
            {
              label: 'API',
              items: ['dev/04-api/convenciones', 'dev/04-api/errores-rfc7807'],
            },
            {
              label: 'Seguridad',
              items: ['dev/05-seguridad/jwt', 'dev/05-seguridad/rbac'],
            },
            {
              label: 'Módulos',
              items: [
                'dev/06-modulos/catalogo',
                'dev/06-modulos/inventario',
                'dev/06-modulos/merma-cortesia',
                'dev/06-modulos/compras',
                'dev/06-modulos/ventas-pos',
                'dev/06-modulos/cuenta-abonos',
                'dev/06-modulos/caja-turnos',
                'dev/06-modulos/club-reservas',
                'dev/06-modulos/crm',
                'dev/06-modulos/finanzas',
                'dev/06-modulos/contenido',
                'dev/06-modulos/ia',
              ],
            },
            {
              label: 'Frontend',
              items: ['dev/07-frontend/overview'],
            },
            {
              label: 'IA',
              items: ['dev/08-ia/casos-uso'],
            },
            {
              label: 'Operaciones',
              items: ['dev/09-operaciones/entornos'],
            },
            {
              label: 'Fases',
              items: ['dev/10-fases/roadmap'],
            },
            {
              label: 'Guías',
              items: ['dev/11-guias/commits', 'dev/11-guias/ramas'],
            },
            {
              label: 'Decisiones (ADR)',
              items: [
                'dev/adr',
                'dev/adr/0001-onion-architecture',
                'dev/adr/0002-postgresql',
                'dev/adr/0003-frontend-react-tailwind',
                'dev/adr/0004-jwt-rbac',
                'dev/adr/0005-ia-en-nube',
                'dev/adr/0006-documentacion-astro-starlight',
                'dev/adr/0007-sucursal-unica',
              ],
            },
            ...openAPISidebarGroups,
          ],
        },
        {
          label: 'Cliente',
          items: [
            'cliente',
            'cliente/reservas',
            'cliente/menu-tasas-horarios',
            'cliente/contacto',
            'cliente/preguntas-frecuentes',
            {
              label: 'Manuales por rol',
              items: [
                'cliente/manuales/mesero',
                'cliente/manuales/cajero',
                'cliente/manuales/barra-cocina',
                'cliente/manuales/administrador',
              ],
            },
          ],
        },
      ],
    }),
    mdx(),
    mermaid({ autoTheme: true }),
    sitemap(),
  ],
});
