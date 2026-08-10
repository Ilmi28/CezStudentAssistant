import { useTranslation } from "react-i18next";
import { RefreshCw, Wifi, LogOut, Sun, Moon, Globe } from "lucide-react";

interface UserControlsProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  onLogout: () => void;
  darkMode: boolean;
  onToggleDarkMode: () => void;
}

export default function UserControls({
  loading,
  onRefresh,
  username,
  onLogout,
  darkMode,
  onToggleDarkMode,
}: UserControlsProps) {
  const { t, i18n } = useTranslation();

  const toggleLanguage = () => {
    const next = i18n.language === "pl" ? "en" : "pl";
    i18n.changeLanguage(next);
    localStorage.setItem("language", next);
  };

  return (
    <div className="flex flex-wrap items-center justify-end gap-4 self-end md:self-auto">
      {/* Status indicator */}
      <div className="flex items-center gap-1.5 text-[11px] text-primary font-mono font-medium">
        <Wifi size={12} className="animate-pulse" /> {t("common.statusOnline")}
      </div>

      {/* Refresh button */}
      <button
        onClick={onRefresh}
        disabled={loading}
        className="p-1.5 rounded bg-white/5 text-white/60 hover:text-white hover:bg-white/10 transition-all border border-white/10 disabled:opacity-50 cursor-pointer"
      >
        <RefreshCw size={14} className={loading ? "animate-spin" : ""} />
      </button>

      {/* Language toggle button */}
      <button
        onClick={toggleLanguage}
        className="flex items-center gap-1.5 px-2 py-1.5 rounded bg-white/5 text-white/60 hover:text-white hover:bg-white/10 transition-all border border-white/10 cursor-pointer text-[11px] font-bold uppercase tracking-wider"
        title={t("common.switchLanguage")}
      >
        <Globe size={13} />
        <span>{t("common.switchLanguageLabel")}</span>
      </button>

      {/* Dark Mode toggle button */}
      <button
        onClick={onToggleDarkMode}
        className="p-1.5 rounded bg-white/5 text-white/60 hover:text-white hover:bg-white/10 transition-all border border-white/10 cursor-pointer"
      >
        {darkMode ? <Sun size={14} /> : <Moon size={14} />}
      </button>

      {/* Logged user info & Logout */}
      <div className="flex items-center gap-2.5 bg-white/5 px-3 py-1.5 rounded border border-white/10 text-white">
        <div className="w-7 h-7 rounded bg-primary text-primary-foreground flex items-center justify-center text-[10px] font-bold flex-shrink-0 uppercase shadow-xs">
          {username ? username.substring(0, 2) : t("common.avatarDefault")}
        </div>
        <span className="text-[12px] font-medium truncate max-w-[100px]" title={username || "Student"}>
          {username || "Student"}
        </span>
        <button
          onClick={onLogout}
          className="text-white/40 hover:text-white/90 transition-colors cursor-pointer ml-1"
          title={t("common.logout")}
        >
          <LogOut size={13} />
        </button>
      </div>
    </div>
  );
}
