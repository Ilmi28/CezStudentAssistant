import React from "react";
import { AlertCircle } from "lucide-react";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string | null;
  hasError?: boolean;
  helperText?: string;
}

export const Input: React.FC<InputProps> = ({
  label,
  error,
  hasError,
  helperText,
  className = "",
  id,
  type = "text",
  ...props
}) => {
  const inputId = id || (label ? `input-${label.toLowerCase().replace(/\s+/g, "-")}` : undefined);

  return (
    <div className="w-full space-y-1.5">
      {label && (
        <label
          htmlFor={inputId}
          className="block text-sm font-medium text-foreground mb-1.5"
        >
          {label}
        </label>
      )}
      <input
        id={inputId}
        type={type}
        className={`
          w-full px-4 py-3 text-sm border rounded-xl bg-card text-foreground font-sans
          transition-all focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary
          [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none
          border-border hover:border-border/80
          ${className}
        `.trim()}
        {...props}
      />
      {error && (
        <p className="text-[11px] text-red-500 dark:text-red-400 font-medium flex items-center gap-1.5 animate-in fade-in duration-150">
          <AlertCircle size={12} className="shrink-0" />
          <span>{error}</span>
        </p>
      )}
      {helperText && !error && <p className="text-[11px] text-muted-foreground">{helperText}</p>}
    </div>
  );
};

export default Input;
