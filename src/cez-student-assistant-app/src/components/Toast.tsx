import React, { useState, useEffect } from "react";
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
  const [shouldRender, setShouldRender] = useState(Boolean(message));
  const [isClosing, setIsClosing] = useState(false);
  const [activeMessage, setActiveMessage] = useState(message);

  useEffect(() => {
    if (message) {
      setActiveMessage(message);
      setShouldRender(true);
      setIsClosing(false);
    } else if (shouldRender && !isClosing) {
      setIsClosing(true);
      const timer = setTimeout(() => {
        setShouldRender(false);
        setIsClosing(false);
        setActiveMessage(null);
      }, 180);
      return () => clearTimeout(timer);
    }
  }, [message]);

  const handleClose = () => {
    if (isClosing) return;
    setIsClosing(true);
    setTimeout(() => {
      onClose();
      setIsClosing(false);
      setShouldRender(false);
      setActiveMessage(null);
    }, 180);
  };

  if (!shouldRender || !activeMessage) return null;

  const isSuccess = variant === "success";

  return (
    <div
      className={`
        fixed bottom-4 right-4 z-50 px-5 py-3.5 rounded-xl shadow-xl flex items-center gap-3 border
        font-medium
        ${isClosing ? "animate-toast-exit" : "animate-toast-enter"}
        ${isSuccess
          ? "bg-primary text-primary-foreground border-primary/30"
          : "bg-destructive text-destructive-foreground border-destructive/30"
        }
      `.trim()}
    >
      {isSuccess ? <CheckCircle size={18} /> : <AlertCircle size={18} />}
      <span className="text-[13px]">{activeMessage}</span>
      <button
        type="button"
        onClick={handleClose}
        className="ml-2 hover:opacity-80 transition-opacity cursor-pointer focus:outline-none"
        aria-label="Close"
      >
        <X size={15} />
      </button>
    </div>
  );
};

export default Toast;
