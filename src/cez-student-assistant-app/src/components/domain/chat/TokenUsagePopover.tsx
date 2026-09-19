import React, { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { CircularProgress } from "../../ui/CircularProgress";
import type { UserUsageDto } from "../../../types";

interface TokenUsagePopoverProps {
  usage: UserUsageDto;
}

export const TokenUsagePopover: React.FC<TokenUsagePopoverProps> = ({ usage }) => {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const used = usage.dailyTokensUsed || 0;
  const reserved = usage.dailyTokensReserved || 0;
  const limit = usage.dailyTokenLimit || 1;
  const pct = usage.dailyUsagePercentage ?? Math.round(((used + reserved) / limit) * 100);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  return (
    <div ref={containerRef} className="relative inline-flex items-center">
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="p-1 rounded-full hover:bg-muted/60 transition-colors focus:outline-hidden cursor-pointer flex items-center justify-center"
        aria-label={t("chat.dailyTokenLimitTitle", "Dzienny limit tokenów")}
      >
        <CircularProgress percentage={pct} size={20} strokeWidth={2.5} />
      </button>

      {isOpen && (
        <div className="absolute left-1/2 -translate-x-1/2 bottom-full mb-2.5 px-3.5 py-2 bg-sidebar border border-border shadow-2xl rounded-xl space-y-0.5 z-50 animate-in fade-in slide-in-from-bottom-2 duration-150 text-center whitespace-nowrap select-none">
          <div className="text-sm font-extrabold text-white leading-tight">
            {pct}%
          </div>
          <div className="text-[11px] text-muted-foreground font-medium">
            {used.toLocaleString()} / {limit.toLocaleString()} {t("courseDetails.tokensUnit", "tokenów")}
          </div>
        </div>
      )}
    </div>
  );
};

export default TokenUsagePopover;
