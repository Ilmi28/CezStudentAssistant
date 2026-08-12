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
  const dropdownRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const { t } = useTranslation();

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

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen]);

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
        className="flex items-center gap-2.5 bg-white/10 hover:bg-white/20 px-3 py-1.5 rounded-lg border border-white/15 text-white transition-all cursor-pointer focus:outline-hidden focus:ring-2 focus:ring-primary/50"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <div className="w-7 h-7 rounded-md bg-primary text-primary-foreground flex items-center justify-center text-[10px] font-bold flex-shrink-0 uppercase shadow-xs">
          {username ? username.substring(0, 2) : t("common.avatarDefault")}
        </div>
        <span className="text-[12px] font-medium truncate max-w-[110px]" title={username || "Student"}>
          {username || "Student"}
        </span>
        <ChevronDown
          size={14}
          className={`text-white/70 transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {/* Popover Menu - Uses popover design tokens for light and dark modes */}
      {isOpen && (
        <div className="absolute right-0 mt-2 w-52 rounded-xl bg-popover text-popover-foreground border border-border shadow-xl py-1.5 z-50 animate-in fade-in zoom-in-95 duration-100">
          {/* User Info Header */}
          <div className="px-4 py-2.5 border-b border-border">
            <p className="text-[11px] font-medium text-muted-foreground uppercase tracking-wider">
              {t("profile.signedInAs")}
            </p>
            <p className="text-xs font-semibold text-foreground truncate mt-0.5">
              {username || "Student"}
            </p>
          </div>

          {/* Menu Options */}
          <div className="py-1">
            <button
              type="button"
              onClick={handlePreferencesClick}
              className="w-full flex items-center gap-2.5 px-4 py-2 text-xs font-medium text-foreground hover:bg-muted transition-colors cursor-pointer text-left"
            >
              <Settings size={15} className="text-primary" />
              <span>{t("profile.preferences")}</span>
            </button>

            <button
              type="button"
              onClick={handleLogoutClick}
              className="w-full flex items-center gap-2.5 px-4 py-2 text-xs font-medium text-destructive hover:bg-destructive/10 transition-colors cursor-pointer text-left"
            >
              <LogOut size={15} />
              <span>{t("common.logout")}</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
