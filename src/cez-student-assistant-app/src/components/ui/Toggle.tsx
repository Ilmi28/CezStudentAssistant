import React from "react";

export interface ToggleProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: React.ReactNode;
  description?: React.ReactNode;
  disabled?: boolean;
  size?: "sm" | "md";
  className?: string;
}

export const Toggle: React.FC<ToggleProps> = ({
  checked,
  onChange,
  label,
  description,
  disabled = false,
  size = "md",
  className = "",
}) => {
  const isSm = size === "sm";

  const trackWidth = isSm ? "w-8 h-4" : "w-10 h-5.5";
  const thumbSize = isSm ? "w-3 h-3" : "w-4 h-4";
  const thumbTranslate = isSm
    ? checked ? "translate-x-4" : "translate-x-0.5"
    : checked ? "translate-x-5" : "translate-x-0.5";

  const handleKeyDown = (e: React.KeyboardEvent<HTMLButtonElement>) => {
    if (disabled) return;
    if (e.key === " " || e.key === "Enter") {
      e.preventDefault();
      onChange(!checked);
    }
  };

  return (
    <div className={`flex items-start justify-between gap-3 ${className}`}>
      {(label || description) && (
        <div className="flex flex-col cursor-pointer select-none" onClick={() => !disabled && onChange(!checked)}>
          {label && (
            <span className="text-xs font-medium text-foreground leading-tight">
              {label}
            </span>
          )}
          {description && (
            <span className="text-[11px] text-muted-foreground leading-relaxed mt-0.5">
              {description}
            </span>
          )}
        </div>
      )}

      <button
        type="button"
        role="switch"
        aria-checked={checked}
        disabled={disabled}
        onClick={() => !disabled && onChange(!checked)}
        onKeyDown={handleKeyDown}
        className={`relative inline-flex flex-shrink-0 items-center rounded-full border transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary/40 ${trackWidth} ${
          checked
            ? "bg-primary border-primary"
            : "bg-secondary border-border"
        } ${disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
      >
        <span
          className={`inline-block rounded-full bg-white shadow-sm transform transition-transform duration-200 ease-in-out ${thumbSize} ${thumbTranslate}`}
        />
      </button>
    </div>
  );
};

export default Toggle;
