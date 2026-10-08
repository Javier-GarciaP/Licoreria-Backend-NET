import {
  Dialog as SheetRoot,
  DialogClose as SheetClose,
  DialogContent as SheetContent,
  DialogDescription as SheetDescription,
  DialogOverlay as SheetOverlay,
  DialogPortal as SheetPortal,
  DialogTitle as SheetTitle,
  DialogTrigger as SheetTrigger,
} from '@radix-ui/react-dialog';

/**
 * Sheet lateral/inferior construido sobre Dialog (Radix).
 * Se usa para la navegación móvil y paneles que deslizan desde abajo.
 */
export const Sheet = SheetRoot;
export { SheetTrigger, SheetClose, SheetPortal, SheetOverlay, SheetContent, SheetTitle, SheetDescription };