import React from "react";
import { AlertCircle, CheckCircle, X } from "lucide-react";

export interface ToastProps {
  variant?: "success" | "error";
  message: string | null;
  onClose: () => void;
}

export const Toast: React.FC<ToastProps> = ({
  variant = "success",
  message,
  onClose
}) => {
  if (!message) return null;

  const isSuccess = variant === "success";

  return (
    <div
      className={`
        fixed bottom-4 right-4 z-50 px-5 py-3.5 rounded-lg shadow-xl flex items-center gap-3 border
        transition-all animate-in slide-in-from-bottom-2 fade-in duration-200 font-medium
        ${isSuccess
          ? "bg-primary text-primary-foreground border-primary/30"
          : "bg-[#c44444] text-white border-[#a23333]"
        }
      `.trim()}
    >
      {isSuccess ? <CheckCircle size={18} /> : <AlertCircle size={18} />}
      <span className="text-[13px]">{message}</span>
      <button
        type="button"
        onClick={onClose}
        className="ml-2 hover:opacity-80 transition-opacity cursor-pointer focus:outline-none"
        aria-label="Close"
      >
        <X size={15} />
      </button>
    </div>
  );
};

export default Toast;
