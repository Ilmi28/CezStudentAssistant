import React from "react";
import { AlertCircle, CheckCircle, Info, AlertTriangle } from "lucide-react";

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
    error: "bg-red-500/10 border-red-500/30 text-red-500 dark:text-red-400",
    success: "bg-emerald-500/10 border-emerald-500/30 text-emerald-500 dark:text-emerald-400",
    warning: "bg-amber-500/10 border-amber-500/30 text-amber-500 dark:text-amber-400",
    info: "bg-blue-500/10 border-blue-500/30 text-blue-500 dark:text-blue-400",
  };

  const icons = {
    error: <AlertCircle size={15} className="shrink-0" />,
    success: <CheckCircle size={15} className="shrink-0" />,
    warning: <AlertTriangle size={15} className="shrink-0" />,
    info: <Info size={15} className="shrink-0" />,
  };

  return (
    <div
      className={`
        border p-3 rounded text-[12px] font-medium flex items-center gap-2.5 animate-in fade-in duration-150
        ${variantStyles[variant]}
        ${className}
      `.trim()}
    >
      {icons[variant]}
      <span>{content}</span>
    </div>
  );
};

export default Alert;
