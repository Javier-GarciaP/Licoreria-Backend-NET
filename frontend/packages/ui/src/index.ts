/* Utilidades */
export { cn } from './lib/cn';
export { useFocusTrap } from './lib/useFocusTrap';

/* Componentes públicos (API estable) */
export { Button } from './components/Button';
export type { ButtonProps } from './components/Button';
export { Card, CardHeader, CardTitle, CardBody } from './components/Card';
export { Input } from './components/Input';
export type { InputProps } from './components/Input';
export { Select } from './components/Select';
export type { SelectProps } from './components/Select';
export { Buscador } from './components/Buscador';
export type { BuscadorProps } from './components/Buscador';
export { FiltroDropdown } from './components/FiltroDropdown';
export type { FiltroDropdownProps, FiltroOpcion } from './components/FiltroDropdown';
export { FiltroPopover } from './components/FiltroPopover';
export type { FiltroPopoverProps } from './components/FiltroPopover';
export { FiltroRango } from './components/FiltroRango';
export type { FiltroRangoProps } from './components/FiltroRango';
export { FiltroFechas } from './components/FiltroFechas';
export type { FiltroFechasProps } from './components/FiltroFechas';
export { LimpiarFiltros } from './components/LimpiarFiltros';
export type { LimpiarFiltrosProps } from './components/LimpiarFiltros';
export { Skeleton, SkeletonCard, SkeletonTable } from './components/Skeleton';
export { Pill, StatusBadge } from './components/Badge';
export { DataTable } from './components/DataTable';
export type { Column, DataTableProps } from './components/DataTable';
export { Pagination } from './components/Pagination';
export type { PaginationProps } from './components/Pagination';
export { PageHeader, EmptyState, Spinner } from './components/Layout';
export { Modal, ModalSection } from './components/Modal';
export type { ModalProps } from './components/Modal';
export { ActionMenu } from './components/ActionMenu';
export type { ActionMenuOption } from './components/ActionMenu';
export { BubbleModal } from './components/BubbleModal';

/* Componentes shadcn/ui (primitivas canónicas) */
export { Button as UiButton, buttonVariants } from './components/ui/button';
export type { ButtonProps as UiButtonProps, ButtonVariant, ButtonSize } from './components/ui/button';
export {
  Card as UiCard,
  CardHeader as UiCardHeader,
  CardTitle as UiCardTitle,
  CardDescription as UiCardDescription,
  CardContent as UiCardContent,
  CardFooter as UiCardFooter,
} from './components/ui/card';
export { Input as UiInput } from './components/ui/input';
export { Label as UiLabel } from './components/ui/label';
export { Badge as UiBadge, badgeVariants } from './components/ui/badge';
export type { BadgeProps as UiBadgeProps, BadgeVariant } from './components/ui/badge';
export { Separator as UiSeparator } from './components/ui/separator';
export type { SeparatorProps as UiSeparatorProps } from './components/ui/separator';
export {
  Table as UiTable,
  TableHeader as UiTableHeader,
  TableBody as UiTableBody,
  TableRow as UiTableRow,
  TableHead as UiTableHead,
  TableCell as UiTableCell,
  TableCaption as UiTableCaption,
} from './components/ui/table';
export { Skeleton as UiSkeleton } from './components/ui/skeleton';
export { Spinner as UiSpinner } from './components/ui/spinner';
export type { SpinnerProps as UiSpinnerProps } from './components/ui/spinner';
export { Pagination as UiPagination } from './components/ui/pagination';
export type { PaginationProps as UiPaginationProps } from './components/ui/pagination';
export {
  Dialog,
  DialogTrigger,
  DialogClose,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
  DialogDescription,
} from './components/ui/dialog';
export {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuPortal,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuGroup,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubTrigger,
  DropdownMenuSubContent,
} from './components/ui/dropdown-menu';
export { TooltipProvider, Tooltip, TooltipTrigger, TooltipContent, TooltipArrow } from './components/ui/tooltip';
export { Tabs, TabsList, TabsTrigger, TabsContent } from './components/ui/tabs';
export { Sheet, SheetTrigger, SheetClose, SheetPortal, SheetOverlay, SheetContent, SheetTitle, SheetDescription } from './components/ui/sheet';
export { Command, CommandDialog, CommandInput, CommandList, CommandEmpty, CommandGroup, CommandItem, CommandSeparator } from './components/ui/command';
export { Avatar as UiAvatar } from './components/ui/avatar';
export type { AvatarProps as UiAvatarProps } from './components/ui/avatar';
export { Alert as UiAlert, alertVariants } from './components/ui/alert';
export type { AlertProps as UiAlertProps, AlertVariant } from './components/ui/alert';
export { Empty as UiEmpty } from './components/ui/empty';
export type { EmptyProps as UiEmptyProps } from './components/ui/empty';
export { Breadcrumb as UiBreadcrumb } from './components/ui/breadcrumb';
export type { BreadcrumbItem as UiBreadcrumbItem } from './components/ui/breadcrumb';