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
  sm: "text-xs font-semibold px-3.5 py-2 rounded-lg",
  md: "text-sm font-semibold px-5 py-2.5 rounded-xl",
  lg: "text-base font-bold px-6 py-3 rounded-xl",
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
        group btn-app-spring rounded-xl cursor-pointer select-none
        bg-primary text-primary-foreground shadow-sm shadow-primary/20
        focus:outline-none focus-visible:ring-2 focus-visible:ring-primary/40
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && (
          <span className="shrink-0 flex items-center justify-center btn-inner-scale">
            {icon}
          </span>
        )
      )}
      {children && (
        <span className="btn-inner-scale">
          {children}
        </span>
      )}
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
        group btn-app-spring rounded-xl cursor-pointer select-none
        bg-secondary/80 border border-border/80 text-foreground shadow-xs
        focus:outline-none focus-visible:ring-2 focus-visible:ring-primary/40
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && (
          <span className="shrink-0 flex items-center justify-center btn-inner-scale">
            {icon}
          </span>
        )
      )}
      {children && (
        <span className="btn-inner-scale">
          {children}
        </span>
      )}
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
      className={`bg-transparent ${className}`}
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
        group btn-app-spring rounded-xl cursor-pointer select-none
        bg-destructive text-destructive-foreground shadow-xs hover:bg-destructive/85 active:bg-destructive/75 transition-all
        focus:outline-none focus-visible:ring-2 focus-visible:ring-destructive/40
        inline-flex items-center justify-center gap-2
        disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none
        ${sizeStyles[size]}
        ${fullWidth ? "w-full" : ""}
        ${className}
      `.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {loading ? (
        <Loader2 size={spinnerSizes[size]} className="animate-spin shrink-0" />
      ) : (
        icon && (
          <span className="shrink-0 flex items-center justify-center btn-inner-scale">
            {icon}
          </span>
        )
      )}
      {children && (
        <span className="btn-inner-scale">
          {children}
        </span>
      )}
    </button>
  );
};

export default PrimaryButton;
