import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import type { FlashcardDeckDto } from "../../../types/flashcardTypes";
import { FlashcardDeckStatusEnum } from "../../../enums/flashcardEnums";
import { Card, Heading, Text, Flex, Tooltip } from "../../index";
import { getScoreColorClass } from "../../../utils/scoreUtils";

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
  const isProcessing = deck.status === FlashcardDeckStatusEnum.Generating || deck.status === FlashcardDeckStatusEnum.Failed;

  const rawTitle = deck.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Fiszki z") || rawTitle === deck.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Fiszki #${index}` : (rawTitle || (index !== undefined ? `Fiszki #${index}` : "Fiszki"));

  return (
    <Card
      hoverEffect={!isProcessing}
      onClick={() => {
        if (!isProcessing) {
          navigate(`/flashcards/${deck.id}`, { state: { fromPath: location.pathname } });
        }
      }}
      className={`p-4 flex-row items-center justify-between gap-3.5 ${className}`}
    >
      <div className="min-w-0 flex-1 space-y-0.5">
        <Flex align="center" gap={2} wrap className="min-w-0">
          <Heading level={4} size="sm" className="leading-snug truncate tile-title-scale">
            {displayTitle}
          </Heading>
        </Flex>
        {showCourseName && deck.courseName && (
          <Text size="xs" variant="muted" className="line-clamp-1 truncate">
            {deck.courseName}
          </Text>
        )}
      </div>

      <Flex direction="col" align="end" justify="center" gap={0.5} className="shrink-0 text-right">
        {isProcessing ? (
          <Tooltip content="Generowanie fiszek...">
            <div
              className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
            >
              <RefreshCw size={15} className="animate-spin" />
            </div>
          </Tooltip>
        ) : (
          <>
            <span
              className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(deck.progressPercentage)}`}
            >
              {deck.progressPercentage ?? 0}%
            </span>
            <Text size="xs" variant="subtitle">
              {t("quizDetails.stats.masteryIndex", "PROGRES")}
            </Text>
          </>
        )}
      </Flex>
    </Card>
  );
}

export default FlashcardDeckCard;
