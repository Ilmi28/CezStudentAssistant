import { useContext, useEffect } from "react";
import { UIContext } from "../contexts/UIContext";
import { userService } from "../services";
import { UserTheme, UserLanguage } from "../types";

export function useUI() {
  const ctx = useContext(UIContext);
  if (!ctx) {
    throw new Error("useUI must be used within a UIProvider");
  }

  const applyTheme = (theme: UserTheme) => {
    ctx.setCurrentTheme(theme);
    let isDark = false;
    if (theme === UserTheme.Dark) {
      isDark = true;
    } else if (theme === UserTheme.Light) {
      isDark = false;
    } else if (theme === UserTheme.System) {
      isDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
    }
    ctx.setDarkMode(isDark);
    if (isDark) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }
  };

  useEffect(() => {
    if (ctx.currentTheme !== UserTheme.System || !window.matchMedia) return;

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

    const handleChange = (e: MediaQueryListEvent) => {
      const isDark = e.matches;
      ctx.setDarkMode(isDark);
      if (isDark) {
        document.documentElement.classList.add("dark");
      } else {
        document.documentElement.classList.remove("dark");
      }
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, [ctx.currentTheme, ctx.setDarkMode]);

  const updateTheme = async (theme: UserTheme) => {
    applyTheme(theme);
    try {
      await userService.updateUserConfiguration({ theme });
    } catch (err) {
      console.debug("[useUI] Failed to auto-save theme preference:", err);
    }
  };

  const updateUserLanguage = async (lang: UserLanguage) => {
    try {
      await userService.updateUserConfiguration({ language: lang });
    } catch (err) {
      console.debug("[useUI] Failed to auto-save language preference:", err);
    }
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
    applyTheme,
    updateTheme,
    updateUserLanguage,
    setError,
    setSuccess,
  };
}
