import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import type { FlashcardDeckDto } from "../types/flashcardTypes";
import { FlashcardDeckStatusEnum } from "../enums/flashcardEnums";
import Card from "./Card";
import { getScoreColorClass } from "../utils/scoreUtils";

interface FlashcardDeckCardProps {
  deck: FlashcardDeckDto;
  index?: number;
  showCourseName?: boolean;
  className?: string;
}

export function FlashcardDeckCard({
  deck,
  index,
  showCourseName = true,
  className = "",
}: FlashcardDeckCardProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const isGenerating = deck.status === FlashcardDeckStatusEnum.Generating;
  const isFailed = deck.status === FlashcardDeckStatusEnum.Failed;

  const rawTitle = deck.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Fiszki z") || rawTitle === deck.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Fiszki #${index}` : (rawTitle || (index !== undefined ? `Fiszki #${index}` : "Fiszki"));

  return (
    <Card
      hoverEffect={!isGenerating && !isFailed}
      onClick={() => {
        if (!isGenerating && !isFailed) {
          navigate(`/flashcards/${deck.id}`, { state: { fromPath: location.pathname } });
        }
      }}
      className={`p-4 flex-row items-center justify-between gap-3.5 ${className}`}
    >
      <div className="min-w-0 flex-1 space-y-0.5">
        <div className="flex items-center gap-2 min-w-0 flex-wrap">
          <h4 className="text-sm md:text-[15px] font-semibold text-foreground leading-snug truncate">
            {displayTitle}
          </h4>
          {isFailed && (
            <span className="text-[11px] font-medium text-destructive bg-destructive/10 px-2 py-0.5 rounded-full border border-destructive/20">
              Błąd generowania (Ponawianie...)
            </span>
          )}
        </div>
        {showCourseName && deck.courseName && (
          <p className="text-xs text-muted-foreground line-clamp-1">
            {deck.courseName}
          </p>
        )}
      </div>

      <div className="flex flex-col items-end justify-center gap-0.5 shrink-0 text-right">
        {isGenerating ? (
          <div
            title="Generowanie fiszek..."
            className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
          >
            <RefreshCw size={15} className="animate-spin" />
          </div>
        ) : isFailed ? (
          <div
            title="Błąd generowania. Zadanie oczekuje na ponowienie..."
            className="w-8 h-8 rounded-full bg-destructive/15 text-destructive flex items-center justify-center shrink-0 border border-destructive/25"
          >
            <RefreshCw size={15} className="animate-spin" />
          </div>
        ) : (
          <>
            <span
              className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(deck.progressPercentage)}`}
            >
              {deck.progressPercentage ?? 0}%
            </span>
            <span className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider">
              {t("quizDetails.stats.masteryIndex", "PROGRES")}
            </span>
          </>
        )}
      </div>
    </Card>
  );
}
