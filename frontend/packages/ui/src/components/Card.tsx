import type { HTMLAttributes } from 'react';
import { Card as CardPrimitivo, CardContent, CardHeader, CardTitle as CardTitlePrimitivo } from './ui/card';

export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <CardPrimitivo className={className} {...props} />;
}

export { CardHeader };

export function CardTitle({ className, ...props }: HTMLAttributes<HTMLHeadingElement>) {
  return <CardTitlePrimitivo className={className} {...props} />;
}

export function CardBody({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <CardContent className={className} {...props} />;
}