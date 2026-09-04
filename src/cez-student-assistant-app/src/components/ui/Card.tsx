import type { ReactNode } from "react";

interface CardProps {
  children: ReactNode;
  className?: string;
  onClick?: () => void;
  borderLeftPrimary?: boolean;
  hoverEffect?: boolean;
}

export default function Card({
  children,
  className = "",
  onClick,
  borderLeftPrimary = false,
  hoverEffect = false,
}: CardProps) {
  return (
    <div
      onClick={onClick}
      className={`bg-card rounded-xl border border-border p-4.5 shadow-sm flex flex-col justify-between select-none ${
        borderLeftPrimary ? "border-l-4 border-l-primary" : ""
      } ${
        hoverEffect ? "group card-app-spring cursor-pointer" : ""
      } ${className}`}
    >
      {children}
    </div>
  );
}
