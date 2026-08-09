import React from "react";

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string | null;
  helperText?: string;
}

export const Input: React.FC<InputProps> = ({
  label,
  error,
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
          className="block text-[11px] uppercase tracking-wider text-muted-foreground font-semibold font-sans"
        >
          {label}
        </label>
      )}
      <input
        id={inputId}
        type={type}
        className={`
          w-full px-3 py-2 text-[13px] border rounded bg-card text-foreground font-mono
          transition-colors focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary
          ${error ? "border-red-500/60 focus:ring-red-500" : "border-border"}
          ${className}
        `.trim()}
        {...props}
      />
      {error && <p className="text-[11px] text-red-500 font-medium">{error}</p>}
      {helperText && !error && <p className="text-[11px] text-muted-foreground">{helperText}</p>}
    </div>
  );
};

export default Input;
