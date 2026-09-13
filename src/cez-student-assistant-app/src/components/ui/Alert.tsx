import React from "react";

export interface AlertProps {
  variant?: "error" | "success" | "info" | "warning";
  message?: string | null;
  children?: React.ReactNode;
  className?: string;
}

export const Alert: React.FC<AlertProps> = ({
  variant = "error",
  message,
  children,
  className = ""
}) => {
  const content = message || children;
  if (!content) return null;

  const variantStyles = {
    error: "bg-red-500/10 dark:bg-red-500/15 border border-red-500/20 text-red-600 dark:text-red-300",
    success: "bg-emerald-500/10 dark:bg-emerald-500/15 border border-emerald-500/20 text-emerald-600 dark:text-emerald-300",
    warning: "bg-amber-500/10 dark:bg-amber-500/15 border border-amber-500/20 text-amber-600 dark:text-amber-300",
    info: "bg-blue-500/10 dark:bg-blue-500/15 border border-blue-500/20 text-blue-600 dark:text-blue-300",
  };

  return (
    <div
      className={`
        px-4 py-2.5 rounded-xl text-[12px] font-medium leading-relaxed
        animate-in fade-in duration-150
        ${variantStyles[variant]}
        ${className}
      `.trim()}
    >
      <span>{content}</span>
    </div>
  );
};

export default Alert;
