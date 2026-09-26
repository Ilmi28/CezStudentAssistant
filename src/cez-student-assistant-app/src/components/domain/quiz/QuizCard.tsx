import { useNavigate, useLocation } from "react-router-dom";
import type { MouseEvent } from "react";
import { useTranslation } from "react-i18next";
import { RefreshCw, Trash2 } from "lucide-react";
import type { QuizDto } from "../../../types";
import { QuizStatusEnum, QuizAttemptStatus } from "../../../enums/quizEnums";
import { Card, Heading, Text, Flex, Tooltip } from "../../index";
import { getScoreColorClass } from "../../../utils/scoreUtils";

interface QuizCardProps {
  quiz: QuizDto;
  index?: number;
  className?: string;
  showCourseName?: boolean;
  onDelete?: (e: MouseEvent) => void;
}

export default function QuizCard({
  quiz,
  index,
  className = "",
  showCourseName = true,
  onDelete,
}: QuizCardProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const isProcessing = quiz.status === QuizStatusEnum.Generating || quiz.status === QuizStatusEnum.Failed;

  const rawTitle = quiz.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Quiz z") || rawTitle === quiz.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Quiz #${index}` : (rawTitle || (index !== undefined ? `Quiz #${index}` : "Quiz"));

  const isInProgress = quiz.lastAttemptStatus === QuizAttemptStatus.InProgress;

  return (
    <Card
      hoverEffect={!isProcessing}
      onClick={() => {
        if (!isProcessing) {
          navigate(`/quiz/${quiz.id}`, { state: { fromPath: location.pathname } });
        }
      }}
      className={`p-3.5 px-4 flex-row items-center justify-between gap-3 sm:gap-4 ${className}`}
    >
      <div className="min-w-0 flex-1 space-y-0.5">
        <Flex align="center" gap={2} className="min-w-0">
          <Heading level={4} size="sm" className="leading-snug line-clamp-1 truncate tile-title-scale" title={displayTitle}>
            {displayTitle}
          </Heading>
        </Flex>
        {showCourseName && quiz.courseName && (
          <Text size="xs" variant="muted" className="line-clamp-1 truncate">
            {quiz.courseName}
          </Text>
        )}
      </div>

      <Flex align="center" gap={3} className="shrink-0 text-right">
        <Flex direction="col" align="end" justify="center" gap={0.5}>
          {isProcessing ? (
            <Tooltip content={t("quizzes.btnGenerating")}>
              <div
                aria-label={t("quizzes.btnGenerating")}
                className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
              >
                <RefreshCw size={15} className="animate-spin" />
              </div>
            </Tooltip>
          ) : isInProgress ? (
            <span className="text-xs font-bold text-amber-500 dark:text-amber-400 uppercase tracking-wider">
              {t("quizDetails.status.inProgress", "W TOKU")}
            </span>
          ) : (
            <>
              <span
                className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(quiz.progressPercentage)}`}
              >
                {quiz.progressPercentage ?? 0}%
              </span>
              <Text size="xs" variant="subtitle">
                {t("quizDetails.stats.masteryIndex", "PROGRES")}
              </Text>
            </>
          )}
        </Flex>

        {onDelete && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(e);
            }}
            title={t("common.delete", "Usuń")}
            aria-label={t("common.delete", "Usuń")}
            className="p-1.5 rounded-lg text-muted-foreground hover:text-destructive hover:bg-muted transition-colors cursor-pointer shrink-0"
          >
            <Trash2 size={16} />
          </button>
        )}
      </Flex>
    </Card>
  );
}
