import React from "react";

export type BadgeVariant = "primary" | "secondary" | "success" | "warning" | "error" | "outline" | "info";
export type BadgeSize = "sm" | "md";

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant;
  size?: BadgeSize;
  uppercase?: boolean;
  children: React.ReactNode;
  icon?: React.ReactNode;
}

const variantStyles: Record<BadgeVariant, string> = {
  primary: "bg-primary/15 text-primary border-primary/30",
  secondary: "bg-secondary text-secondary-foreground border-border",
  success: "bg-emerald-500/15 text-emerald-400 border-emerald-500/30",
  warning: "bg-amber-500/15 text-amber-400 border-amber-500/30",
  error: "bg-rose-500/15 text-rose-400 border-rose-500/30",
  outline: "bg-transparent text-foreground border-border",
  info: "bg-sky-500/15 text-sky-400 border-sky-500/30",
};

const sizeStyles: Record<BadgeSize, string> = {
  sm: "text-[11px] font-semibold px-2 py-0.5 rounded-md",
  md: "text-xs font-semibold px-2.5 py-1 rounded-lg",
};

export const Badge: React.FC<BadgeProps> = ({
  variant = "primary",
  size = "sm",
  uppercase = false,
  children,
  icon,
  className = "",
  ...props
}) => {
  return (
    <span
      className={`inline-flex items-center gap-1.5 border tracking-wide select-none transition-all duration-150 ${
        uppercase ? "uppercase" : ""
      } ${variantStyles[variant]} ${sizeStyles[size]} ${className}`.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>}
      <span>{children}</span>
    </span>
  );
};

export default Badge;
