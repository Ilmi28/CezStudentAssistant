import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw, Wifi, LogOut, BookOpen, Sun, Moon, Home, Layers, Brain, Globe } from "lucide-react";

interface TopbarProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  onLogout: () => void;
  darkMode: boolean;
  onToggleDarkMode: () => void;
}

export default function Topbar({
  loading,
  onRefresh,
  username,
  onLogout,
  darkMode,
  onToggleDarkMode
}: TopbarProps) {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();

  const isHomeActive = pathname === "/home" || pathname === "/";
  const isCoursesActive = pathname === "/courses" || pathname.startsWith("/course/");
  const isQuizzesActive = pathname === "/quizzes" || pathname.startsWith("/quiz/");

  const toggleLanguage = () => {
    const next = i18n.language === "pl" ? "en" : "pl";
    i18n.changeLanguage(next);
    localStorage.setItem("language", next);
  };

  return (
    <div className="bg-sidebar px-8 py-4 flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-sidebar-border sticky top-0 z-30">
      {/* Left side: Brand Logo & Navigation links aligned together */}
      <div className="flex flex-col md:flex-row md:items-center gap-6 lg:gap-8">
        {/* Brand Logo & Name */}
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded bg-[#006633] border-2 border-white/20 flex items-center justify-center flex-shrink-0">
            <BookOpen size={16} className="text-white" />
          </div>
          <div>
            <div style={{ fontFamily: "Roboto Slab, serif" }} className="text-white font-semibold text-[15px] leading-tight">
              CEZStudentAssistant
            </div>
            <div className="text-[9px] tracking-[0.15em] uppercase text-white/40 mt-0.5 font-medium">
              Politechnika Białostocka
            </div>
          </div>
        </div>

        {/* Horizontal Navigation Links */}
        <nav className="flex items-center gap-5 lg:gap-6 border-t md:border-t-0 border-white/5 pt-2 md:pt-0">
          <button
            onClick={() => navigate("/home")}
            className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
              isHomeActive
                ? "border-white text-white"
                : "border-transparent text-white/60 hover:text-white/95"
            }`}
          >
            <Home size={13} />
            <span>{t("nav.home")}</span>
          </button>
          <button
            onClick={() => navigate("/courses")}
            className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
              isCoursesActive
                ? "border-white text-white"
                : "border-transparent text-white/60 hover:text-white/95"
            }`}
          >
            <Layers size={13} />
            <span>{t("nav.courses")}</span>
          </button>
          <button
            onClick={() => navigate("/quizzes")}
            className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
              isQuizzesActive
                ? "border-white text-white"
                : "border-transparent text-white/60 hover:text-white/95"
            }`}
          >
            <Brain size={13} />
            <span>{t("nav.quizzes")}</span>
          </button>
        </nav>
      </div>

      {/* Right controls */}
      <div className="flex flex-wrap items-center justify-end gap-4 self-end md:self-auto">
        {/* Status indicator */}
        <div className="flex items-center gap-1.5 text-[11px] text-[#00cc66] font-mono font-medium">
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
          title={i18n.language === "pl" ? "Switch to English" : "Przełącz na polski"}
        >
          <Globe size={13} />
          <span>{i18n.language === "pl" ? "EN" : "PL"}</span>
        </button>

        {/* Dark Mode toggle button */}
        <button
          onClick={onToggleDarkMode}
          className="p-1.5 rounded bg-white/5 text-white/60 hover:text-white hover:bg-white/10 transition-all border border-white/10 cursor-pointer"
        >
          {darkMode ? <Sun size={14} /> : <Moon size={14} />}
        </button>

        {/* Logged user info & Logout */}
        <div className="flex items-center gap-2.5 bg-[#002e17]/35 px-3 py-1.5 rounded border border-white/10 text-white">
          <div className="w-7 h-7 rounded bg-[#006633] border border-white/15 flex items-center justify-center text-[10px] font-bold flex-shrink-0 uppercase">
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
    </div>
  );
}
