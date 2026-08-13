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
  sm: "text-xs font-medium px-3.5 py-2 rounded-lg",
  md: "text-sm font-medium px-5 py-3 rounded-xl",
  lg: "text-base font-semibold px-6 py-3.5 rounded-xl",
};

const spinnerSizes = {
  sm: 14,
  md: 18,
  lg: 20,
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
        font-medium rounded-xl transition-all duration-150 cursor-pointer select-none
        bg-primary hover:bg-primary/90 active:scale-[0.98] text-white shadow-sm hover:shadow-md
        focus:outline-none
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:active:scale-100
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>
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
        font-medium rounded-xl transition-all duration-150 cursor-pointer select-none
        bg-card border border-border hover:border-muted-foreground/30 hover:bg-muted/80 active:scale-[0.98] text-foreground shadow-xs
        focus:outline-none
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:active:scale-100
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>
      )}
      <span>{children}</span>
    </button>
  );
};

export default PrimaryButton;
