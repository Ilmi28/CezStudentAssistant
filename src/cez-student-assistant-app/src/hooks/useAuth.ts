import { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AuthContext } from "../contexts/AuthContext";
import { authService, cezService, UnauthorizedError } from "../services";
import { useUI } from "./useUI";

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  const navigate = useNavigate();
  const { t } = useTranslation();
  const { setError, setLoading, setShowCezModal } = useUI();

  const checkAuthStatus = async () => {
    try {
      const storedUser = localStorage.getItem("username") || "Student";
      ctx.setUsername(storedUser);
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        ctx.setUsername(null);
      } else {
        setError(t("common.errorConnection"));
      }
    } finally {
      ctx.setIsAuthChecking(false);
    }
  };

  useEffect(() => {
    checkAuthStatus();
  }, []);

  const handleLoginSuccess = async (user: string) => {
    localStorage.setItem("username", user);
    ctx.setUsername(user);
    navigate("/home");
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
    } catch (logoutErr) {
      console.debug("[useAuth] Logout server call failed, proceeding with local cleanup:", logoutErr);
    }
    localStorage.removeItem("username");
    ctx.setUsername(null);
    ctx.setIsCezConnected(false);
    ctx.setLastCezSync(null);
    document.cookie = "accessToken=; Max-Age=0; path=/;";
    document.cookie = "refreshToken=; Max-Age=0; path=/;";
    navigate("/login");
  };

  const handleCezLinkSubmit = async (cezUser: string, cezPass: string) => {
    setLoading(true);
    try {
      if (ctx.username) {
        if (ctx.isCezConnected) {
          throw new Error(t("preferences.alreadyConnectedError"));
        }
        await cezService.connectCez(cezUser, cezPass);
      } else {
        await authService.loginCez(cezUser, cezPass);
        localStorage.setItem("username", cezUser);
        ctx.setUsername(cezUser);
      }
      ctx.setIsCezConnected(true);
      setShowCezModal(false);
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        return;
      }
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleCezDisconnect = async () => {
    setLoading(true);
    try {
      await cezService.disconnectCez();
      ctx.setIsCezConnected(false);
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        return;
      }
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    username: ctx.username,
    isAuthenticated: ctx.username !== null,
    isAuthChecking: ctx.isAuthChecking,
    isCezConnected: ctx.isCezConnected,
    lastCezSync: ctx.lastCezSync,
    setIsCezConnected: ctx.setIsCezConnected,
    setLastCezSync: ctx.setLastCezSync,
    handleLoginSuccess,
    handleLogout,
    handleCezLinkSubmit,
    handleCezDisconnect,
    checkAuthStatus,
  };
}
