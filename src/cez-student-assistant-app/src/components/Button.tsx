import React from "react";
import { Loader2 } from "lucide-react";

export interface BaseButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  size?: "sm" | "md" | "lg";
  loading?: boolean;
  fullWidth?: boolean;
  icon?: React.ReactNode;
  children: React.ReactNode;
}

const sizeStyles = {
  sm: "text-[12px] px-3 py-1.5",
  md: "text-[13px] px-4 py-2.5",
  lg: "text-[14px] px-6 py-3",
};

const spinnerSizes = {
  sm: 13,
  md: 15,
  lg: 16,
};

/**
 * Główny zielony przycisk akcji (Główny przycisk aplikacji)
 */
export const PrimaryButton: React.FC<BaseButtonProps> = ({
  size = "md",
  loading = false,
  fullWidth = false,
  icon,
  children,
  className = "",
  disabled,
  ...props
}) => {
  return (
    <button
      disabled={disabled || loading}
      className={`
        font-medium rounded transition-all cursor-pointer select-none
        bg-primary hover:bg-primary/90 text-white shadow-sm
        inline-flex items-center justify-center gap-2
        disabled:opacity-75 disabled:cursor-not-allowed
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0">{icon}</span>
      )}
      <span>{children}</span>
    </button>
  );
};

/**
 * Drugorzędny/ramkowy przycisk (np. "Zaloguj przez CEZ", anulowanie)
 */
export const SecondaryButton: React.FC<BaseButtonProps> = ({
  size = "md",
  loading = false,
  fullWidth = false,
  icon,
  children,
  className = "",
  disabled,
  ...props
}) => {
  return (
    <button
      disabled={disabled || loading}
      className={`
        font-medium rounded transition-all cursor-pointer select-none
        border border-border hover:border-primary/40 hover:bg-muted/50 text-foreground shadow-xs
        inline-flex items-center justify-center gap-2
        disabled:opacity-75 disabled:cursor-not-allowed
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0">{icon}</span>
      )}
      <span>{children}</span>
    </button>
  );
};

export default PrimaryButton;
