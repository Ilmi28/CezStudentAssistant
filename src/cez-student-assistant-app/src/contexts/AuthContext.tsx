import { createContext, useState, type ReactNode } from "react";

export interface AuthContextState {
  username: string | null;
  setUsername: (user: string | null) => void;
  isAuthChecking: boolean;
  setIsAuthChecking: (checking: boolean) => void;
  isCezConnected: boolean;
  setIsCezConnected: (connected: boolean) => void;
  lastCezSync: string | null;
  setLastCezSync: (sync: string | null) => void;
}

export const AuthContext = createContext<AuthContextState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [username, setUsername] = useState<string | null>(null);
  const [isAuthChecking, setIsAuthChecking] = useState(true);

  const [isCezConnected, setIsCezConnectedState] = useState<boolean>(() => {
    return localStorage.getItem("isCezConnected") === "true";
  });

  const [lastCezSync, setLastCezSyncState] = useState<string | null>(() => {
    return localStorage.getItem("lastCezSync");
  });

  const setIsCezConnected = (connected: boolean) => {
    setIsCezConnectedState(connected);
    localStorage.setItem("isCezConnected", connected ? "true" : "false");
  };

  const setLastCezSync = (sync: string | null) => {
    setLastCezSyncState(sync);
    if (sync) {
      localStorage.setItem("lastCezSync", sync);
    } else {
      localStorage.removeItem("lastCezSync");
    }
  };

  return (
    <AuthContext.Provider
      value={{
        username,
        setUsername,
        isAuthChecking,
        setIsAuthChecking,
        isCezConnected,
        setIsCezConnected,
        lastCezSync,
        setLastCezSync,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
