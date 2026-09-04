import React from "react";
import { Loader2 } from "lucide-react";

export interface BaseButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  size?: "sm" | "md" | "lg";
  loading?: boolean;
  fullWidth?: boolean;
  icon?: React.ReactNode;
  children?: React.ReactNode;
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
        bg-primary hover:bg-primary/90 active:scale-[0.98] text-primary-foreground shadow-sm shadow-primary/20
        focus:outline-none
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:active:scale-100
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>
      )}
      {children && <span>{children}</span>}
    </button>
  );
};

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
        bg-secondary/80 border border-border/80 hover:border-primary/45 hover:bg-secondary active:scale-[0.98] text-foreground shadow-xs
        focus:outline-none
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:active:scale-100
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>
      )}
      {children && <span>{children}</span>}
    </button>
  );
};

export const OutlineButton: React.FC<BaseButtonProps> = ({
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
    <SecondaryButton
      size={size}
      loading={loading}
      fullWidth={fullWidth}
      icon={icon}
      className={`bg-transparent hover:bg-secondary ${className}`}
      disabled={disabled}
      {...props}
    >
      {children}
    </SecondaryButton>
  );
};

export const DangerButton: React.FC<BaseButtonProps> = ({
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
        bg-destructive hover:bg-destructive/90 active:scale-[0.98] text-destructive-foreground shadow-xs
        focus:outline-none
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:active:scale-100
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && <span className="shrink-0 flex items-center justify-center">{icon}</span>
      )}
      {children && <span>{children}</span>}
    </button>
  );
};

export default PrimaryButton;
