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
  const [isCezConnected, setIsCezConnected] = useState(false);
  const [lastCezSync, setLastCezSync] = useState<string | null>(null);

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
