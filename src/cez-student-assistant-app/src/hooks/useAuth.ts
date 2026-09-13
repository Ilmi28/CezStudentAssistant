import { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AuthContext } from "../contexts/AuthContext";
import { authService, cezService, userService, UnauthorizedError } from "../services";
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
      const [profile, cezStatus] = await Promise.all([
        userService.getUserProfile().catch(() => null),
        cezService.getCezStatus().catch(() => null),
      ]);
      if (profile) {
        if (profile.userName) {
          ctx.setUsername(profile.userName);
          localStorage.setItem("username", profile.userName);
        }
        ctx.setFullName(profile.fullName || null);
      }
      if (cezStatus) {
        ctx.setIsCezConnected(cezStatus.isConnected);
        ctx.setLastCezSync(cezStatus.lastSyncAt ?? null);
      }
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        ctx.setUsername(null);
        ctx.setFullName(null);
        ctx.setIsCezConnected(false);
        ctx.setLastCezSync(null);
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
    const profile = await userService.getUserProfile().catch(() => null);
    if (profile?.fullName) {
      ctx.setFullName(profile.fullName);
    }
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
    ctx.setFullName(null);
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
      await checkAuthStatus();
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
      await checkAuthStatus();
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

  const displayName = ctx.fullName?.trim() || ctx.username || t("preferences.defaultUsername");

  return {
    username: ctx.username,
    fullName: ctx.fullName,
    displayName,
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
