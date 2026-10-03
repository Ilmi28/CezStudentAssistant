import { useState, useRef, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Menu, X } from "lucide-react";
import Navbar from "./Navbar";
import UserControls from "./UserControls";

interface TopbarProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  fullName?: string | null;
  onLogout: () => void;
}

export default function Topbar({
  loading,
  onRefresh,
  username,
  fullName,
  onLogout,
}: TopbarProps) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const mobileNavRef = useRef<HTMLDivElement>(null);
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const navItems = [
    {
      path: "/home",
      label: t("nav.home"),
      isActive: pathname === "/home" || pathname === "/",
    },
    {
      path: "/courses",
      label: t("nav.courses"),
      isActive: pathname === "/courses" || pathname.startsWith("/course/"),
    },
    {
      path: "/quizzes",
      label: t("nav.quizzes"),
      isActive: pathname === "/quizzes" || pathname.startsWith("/quiz/"),
    },
    {
      path: "/flashcards",
      label: t("nav.flashcards", "Fiszki"),
      isActive: pathname === "/flashcards" || pathname.startsWith("/flashcard"),
    },
    {
      path: "/chats",
      label: t("nav.chats", "Czaty"),
      isActive: pathname === "/chats" || pathname.startsWith("/chat"),
    },
  ];

  const handleMobileNavClick = (path: string) => {
    setMobileMenuOpen(false);
    navigate(path);
  };

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (mobileNavRef.current && !mobileNavRef.current.contains(event.target as Node)) {
        setMobileMenuOpen(false);
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setMobileMenuOpen(false);
      }
    }

    if (mobileMenuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [mobileMenuOpen]);

  return (
    <header className="bg-sidebar border-b border-sidebar-border sticky top-0 z-40 shadow-md" ref={mobileNavRef}>
      {/* DESKTOP HEADER VIEW (md and up) - 100% Original Layout */}
      <div className="hidden md:flex md:items-center md:justify-between px-8 h-[72px] gap-4">
        <Navbar />
        <UserControls
          loading={loading}
          onRefresh={onRefresh}
          username={username}
          fullName={fullName}
          onLogout={onLogout}
        />
      </div>

      {/* MOBILE HEADER VIEW (< md) - Sleek 1-line topbar */}
      <div className="flex md:hidden items-center justify-between px-3 sm:px-4 h-[56px] sm:h-[60px] gap-2 sm:gap-3 w-full max-w-full">
        {/* Compact Mobile Brand Logo */}
        <Navbar isMobileCompact />

        {/* Right Section: User Controls + Mobile Menu Toggle */}
        <div className="flex items-center gap-2 shrink-0">
          <UserControls
            loading={loading}
            onRefresh={onRefresh}
            username={username}
            fullName={fullName}
            onLogout={onLogout}
          />
          <button
            type="button"
            onClick={() => setMobileMenuOpen((prev) => !prev)}
            className="group btn-app-spring p-2 text-white/80 hover:text-white bg-white/10 hover:bg-white/15 border border-white/15 rounded-xl transition-colors cursor-pointer select-none shrink-0 flex items-center justify-center w-9 h-9"
            aria-label="Toggle navigation menu"
            aria-expanded={mobileMenuOpen}
          >
            <span
              className={`inline-flex transition-transform duration-300 ease-out ${
                mobileMenuOpen ? "rotate-180 scale-105 text-white" : "rotate-0 scale-100"
              }`}
            >
              {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
            </span>
          </button>
        </div>
      </div>

      {/* MOBILE NAV DRAWER (< md) - Smooth CSS Grid Accordion */}
      <div
        className={`md:hidden mobile-drawer-accordion ${
          mobileMenuOpen ? "is-expanded" : "is-collapsed"
        }`}
      >
        <div className="overflow-hidden min-h-0">
          <div className="border-t border-sidebar-border bg-sidebar/95 backdrop-blur-md px-4 py-3 space-y-1.5 shadow-2xl">
            {navItems.map((item) => (
              <button
                key={item.path}
                onClick={() => handleMobileNavClick(item.path)}
                className={`w-full flex items-center px-4 py-2.5 rounded-xl text-sm font-semibold transition-all cursor-pointer select-none text-left ${
                  item.isActive
                    ? "bg-white/20 text-white font-bold border border-white/25 shadow-xs"
                    : "text-white/80 hover:text-white hover:bg-white/10 border border-transparent"
                }`}
              >
                <span>{item.label}</span>
              </button>
            ))}
          </div>
        </div>
      </div>
    </header>
  );
}
