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
        className="group btn-app-spring flex items-center gap-2.5 px-3.5 py-2.5 text-foreground rounded-xl border border-border/80 bg-secondary/80 shadow-xs cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 select-none"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <div className="w-7 h-7 rounded-full bg-primary/15 text-primary border border-primary/25 flex items-center justify-center text-xs font-bold shrink-0 uppercase">
          {username ? username.substring(0, 2) : t("common.avatarDefault")}
        </div>
        <span className="text-sm font-medium truncate max-w-[140px] inline-block transition-transform duration-200 group-hover:scale-[1.03]" title={username || "Student"}>
          {username || "Student"}
        </span>
        <ChevronDown
          size={16}
          className={`text-muted-foreground transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {/* Popover Menu */}
      {shouldRender && (
        <div
          className={`absolute right-0 mt-2 w-56 rounded-xl bg-card text-card-foreground border border-border shadow-xl p-2 z-50 ${
            isClosing ? "animate-dropdown-exit" : "animate-dropdown-enter"
          }`}
        >
          {/* User Info Header */}
          <div className="px-3 py-2 border-b border-border/60 mb-2">
            <p className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider">
              {t("profile.signedInAs")}
            </p>
            <p className="text-sm font-semibold text-foreground truncate mt-0.5">
              {username || "Student"}
            </p>
          </div>

          {/* Menu Options */}
          <div className="space-y-1.5">
            <button
              type="button"
              onClick={handlePreferencesClick}
              className="group btn-app-spring w-full flex items-center gap-2.5 px-3.5 py-2.5 rounded-xl border border-border/80 bg-secondary/80 text-foreground shadow-xs cursor-pointer text-left select-none"
            >
              <Settings size={16} className="text-primary shrink-0 group-hover:scale-110 transition-transform duration-200" />
              <span className="text-sm font-medium inline-block transition-transform duration-200 group-hover:scale-[1.03]">
                {t("profile.preferences")}
              </span>
            </button>

            <button
              type="button"
              onClick={handleLogoutClick}
              className="group btn-app-spring w-full flex items-center gap-2.5 px-3.5 py-2.5 rounded-xl border border-border/80 bg-secondary/80 text-foreground shadow-xs cursor-pointer text-left select-none"
            >
              <LogOut size={16} className="text-muted-foreground shrink-0 group-hover:scale-110 transition-transform duration-200" />
              <span className="text-sm font-medium inline-block transition-transform duration-200 group-hover:scale-[1.03]">
                {t("common.logout")}
              </span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
