import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Settings, LogOut, ChevronDown } from "lucide-react";
import { Card, Flex, Text } from "../index";

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
        className="group btn-app-spring flex items-center gap-2 md:gap-2.5 px-2.5 sm:px-3 py-1.5 sm:py-2 text-white rounded-xl border border-white/15 bg-white/10 hover:bg-white/15 shadow-xs cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-white/40 select-none"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <Flex align="center" justify="center" className="w-7 h-7 rounded-full bg-white/20 text-white border border-white/30 text-xs font-bold shrink-0 uppercase">
          {avatarText}
        </Flex>
        <Text size="sm" className="hidden md:inline-block font-semibold text-white truncate max-w-[140px]">
          {displayName}
        </Text>
        <ChevronDown
          size={15}
          className={`text-white/70 group-hover:text-white transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {shouldRender && (
        <Card
          className={`absolute right-0 mt-2 w-60 shadow-2xl p-1.5 z-50 border-border/80 ${
            isClosing ? "animate-dropdown-exit" : "animate-dropdown-enter"
          }`}
        >
          <div className="flex items-center gap-3 px-3 py-2.5 border-b border-border/60 mb-1">
            <Flex align="center" justify="center" className="w-9 h-9 rounded-full bg-primary/20 text-primary border border-primary/30 text-xs font-bold shrink-0 uppercase">
              {avatarText}
            </Flex>
            <div className="min-w-0 flex-1">
              <Text size="sm" className="font-bold text-foreground truncate block">
                {displayName}
              </Text>
              {fullName?.trim() && username && (
                <Text size="xs" variant="muted" className="truncate block">
                  @{username}
                </Text>
              )}
            </div>
          </div>

          <div className="space-y-0.5">
            <button
              type="button"
              onClick={handlePreferencesClick}
              className="w-full flex items-center gap-2.5 px-3 py-2 text-xs font-semibold text-foreground rounded-lg hover:bg-muted/70 active:bg-muted transition-colors cursor-pointer select-none text-left"
            >
              <Settings size={15} className="text-primary shrink-0" />
              <span>{t("profile.preferences")}</span>
            </button>

            <button
              type="button"
              onClick={handleLogoutClick}
              className="w-full flex items-center gap-2.5 px-3 py-2 text-xs font-semibold text-destructive rounded-lg hover:bg-destructive/10 active:bg-destructive/15 transition-colors cursor-pointer select-none text-left"
            >
              <LogOut size={15} className="text-destructive shrink-0" />
              <span>{t("common.logout")}</span>
            </button>
          </div>
        </Card>
      )}
    </div>
  );
}
