import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Settings, LogOut, ChevronDown } from "lucide-react";
import { Card, Flex, Text, SecondaryButton } from "../index";

interface ProfileDropdownProps {
  username: string | null;
  fullName?: string | null;
  onLogout: () => void;
}

export default function ProfileDropdown({ username, fullName, onLogout }: ProfileDropdownProps) {
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

  const defaultUserText = t("preferences.defaultUsername");
  const displayName = fullName?.trim() || username || defaultUserText;

  const getInitials = (name: string) => {
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  };

  const avatarText = getInitials(displayName);

  return (
    <div className="relative inline-block text-left" ref={dropdownRef}>
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="group btn-app-spring flex items-center gap-2.5 px-3.5 py-2.5 text-white rounded-xl border border-white/15 bg-white/10 hover:bg-white/15 shadow-xs cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-white/40 select-none"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <Flex align="center" justify="center" className="w-7 h-7 rounded-full bg-white/20 text-white border border-white/30 text-xs font-bold shrink-0 uppercase">
          {avatarText}
        </Flex>
        <Text size="sm" className="font-semibold text-white truncate max-w-[140px] inline-block transition-transform duration-200 group-hover:scale-[1.03]" title={displayName}>
          {displayName}
        </Text>
        <ChevronDown
          size={16}
          className={`text-white/70 group-hover:text-white transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {shouldRender && (
        <Card
          className={`absolute right-0 mt-2 w-56 shadow-xl p-2 z-50 ${
            isClosing ? "animate-dropdown-exit" : "animate-dropdown-enter"
          }`}
        >
          <div className="px-3 py-2 border-b border-border/60 mb-2">
            <Text variant="subtitle" size="xs">
              {t("profile.signedInAs")}
            </Text>
            <Text size="sm" className="font-semibold text-foreground truncate mt-0.5">
              {displayName}
            </Text>
            {fullName?.trim() && username && (
              <Text size="xs" variant="muted" className="truncate block mt-0.5">
                @{username}
              </Text>
            )}
          </div>

          <div className="space-y-1">
            <SecondaryButton
              type="button"
              fullWidth
              onClick={handlePreferencesClick}
              icon={<Settings size={16} className="text-primary shrink-0 group-hover:scale-110 transition-transform duration-200" />}
              className="justify-start px-3 py-2 text-foreground hover:bg-muted font-medium bg-transparent border-none shadow-none"
            >
              {t("profile.preferences")}
            </SecondaryButton>

            <SecondaryButton
              type="button"
              fullWidth
              onClick={handleLogoutClick}
              icon={<LogOut size={16} className="text-destructive shrink-0 group-hover:scale-110 transition-transform duration-200" />}
              className="justify-start px-3 py-2 text-destructive hover:bg-destructive/10 font-medium bg-transparent border-none shadow-none"
            >
              {t("common.logout")}
            </SecondaryButton>
          </div>
        </Card>
      )}
    </div>
  );
}
