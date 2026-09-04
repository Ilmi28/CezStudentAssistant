import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Settings, LogOut, ChevronDown } from "lucide-react";

interface ProfileDropdownProps {
  username: string | null;
  onLogout: () => void;
}

export default function ProfileDropdown({ username, onLogout }: ProfileDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [shouldRender, setShouldRender] = useState(false);
  const [isClosing, setIsClosing] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const { t } = useTranslation();

  useEffect(() => {
    if (isOpen) {
      setShouldRender(true);
      setIsClosing(false);
    } else if (shouldRender && !isClosing) {
      setIsClosing(true);
      const timer = setTimeout(() => {
        setShouldRender(false);
        setIsClosing(false);
      }, 140);
      return () => clearTimeout(timer);
    }
  }, [isOpen]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    }

    if (shouldRender) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [shouldRender]);

  const handlePreferencesClick = () => {
    setIsOpen(false);
    navigate("/preferences");
  };

  const handleLogoutClick = () => {
    setIsOpen(false);
    onLogout();
  };

  return (
    <div className="relative inline-block text-left" ref={dropdownRef}>
      {/* Profile Trigger Button */}
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className={`flex items-center gap-3 px-4 py-2.5 text-white transition-all cursor-pointer focus:outline-none ${
          isOpen
            ? "rounded-t-xl rounded-b-none border border-white/20 bg-white/15 relative z-20"
            : "rounded-xl border border-white/15 bg-white/10 hover:bg-white/20 shadow-xs"
        }`}
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <div className="w-8 h-8 rounded-lg bg-primary text-primary-foreground flex items-center justify-center text-xs font-bold flex-shrink-0 uppercase shadow-xs">
          {username ? username.substring(0, 2) : t("common.avatarDefault")}
        </div>
        <span className="text-sm font-medium truncate max-w-[140px]" title={username || "Student"}>
          {username || "Student"}
        </span>
        <ChevronDown
          size={16}
          className={`text-white/70 transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {/* Popover Menu */}
      {shouldRender && (
        <div
          className={`absolute right-0 top-full -mt-px w-56 rounded-b-xl rounded-tl-xl bg-card text-card-foreground border border-border shadow-2xl overflow-hidden z-50 ${
            isClosing ? "animate-dropdown-exit" : "animate-dropdown-enter"
          }`}
        >
          {/* User Info Header */}
          <div className="px-4 py-3 border-b border-border bg-muted/30">
            <p className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider">
              {t("profile.signedInAs")}
            </p>
            <p className="text-sm font-semibold text-foreground truncate mt-0.5">
              {username || "Student"}
            </p>
          </div>

          {/* Menu Options */}
          <div>
            <button
              type="button"
              onClick={handlePreferencesClick}
              className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-foreground hover:bg-muted transition-colors cursor-pointer text-left"
            >
              <Settings size={16} className="text-primary" />
              <span>{t("profile.preferences")}</span>
            </button>

            <button
              type="button"
              onClick={handleLogoutClick}
              className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-destructive hover:bg-destructive/15 transition-colors cursor-pointer text-left last:rounded-b-xl"
            >
              <LogOut size={16} />
              <span>{t("common.logout")}</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
