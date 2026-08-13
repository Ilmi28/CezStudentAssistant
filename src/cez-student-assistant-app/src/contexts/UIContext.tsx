import { createContext, useState, useEffect, type ReactNode } from "react";
import { UserTheme } from "../types";

export interface UIContextType {
  currentTheme: UserTheme;
  setCurrentTheme: React.Dispatch<React.SetStateAction<UserTheme>>;
  darkMode: boolean;
  setDarkMode: React.Dispatch<React.SetStateAction<boolean>>;
  loading: boolean;
  setLoading: (loading: boolean) => void;
  syncing: boolean;
  setSyncing: (syncing: boolean) => void;
  showCezModal: boolean;
  setShowCezModal: (show: boolean) => void;
  errorMsg: string | null;
  setErrorMsg: (msg: string | null) => void;
  successMsg: string | null;
  setSuccessMsg: (msg: string | null) => void;
}

export const UIContext = createContext<UIContextType | null>(null);

export function UIProvider({ children }: { children: ReactNode }) {
  const [currentTheme, setCurrentTheme] = useState<UserTheme>(UserTheme.Dark);
  const [darkMode, setDarkMode] = useState<boolean>(() => {
    const stored = localStorage.getItem("darkMode");
    return stored !== null ? stored === "true" : true;
  });

  const [loading, setLoading] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [showCezModal, setShowCezModal] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add("dark");
      localStorage.setItem("darkMode", "true");
    } else {
      document.documentElement.classList.remove("dark");
      localStorage.setItem("darkMode", "false");
    }
  }, [darkMode]);

  return (
    <UIContext.Provider
      value={{
        currentTheme,
        setCurrentTheme,
        darkMode,
        setDarkMode,
        loading,
        setLoading,
        syncing,
        setSyncing,
        showCezModal,
        setShowCezModal,
        errorMsg,
        setErrorMsg,
        successMsg,
        setSuccessMsg,
      }}
    >
      {children}
    </UIContext.Provider>
  );
}
