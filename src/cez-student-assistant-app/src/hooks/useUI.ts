import { useContext } from "react";
import { UIContext } from "../contexts/UIContext";

export function useUI() {
  const ctx = useContext(UIContext);
  if (!ctx) {
    throw new Error("useUI must be used within a UIProvider");
  }

  const toggleDarkMode = () => {
    ctx.setDarkMode((prev) => !prev);
  };

  const setError = (msg: string) => {
    ctx.setErrorMsg(msg);
    setTimeout(() => ctx.setErrorMsg(null), 5000);
  };

  const setSuccess = (msg: string) => {
    ctx.setSuccessMsg(msg);
    setTimeout(() => ctx.setSuccessMsg(null), 5000);
  };

  return {
    ...ctx,
    toggleDarkMode,
    setError,
    setSuccess,
  };
}
