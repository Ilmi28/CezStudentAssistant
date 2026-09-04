import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import type { QuizDto } from "../../../types";
import { QuizStatusEnum, QuizAttemptStatus } from "../../../enums/quizEnums";
import { Card, Badge, Heading, Text, Flex, Tooltip } from "../../index";
import { getScoreColorClass } from "../../../utils/scoreUtils";

interface QuizCardProps {
  quiz: QuizDto;
  index?: number;
  className?: string;
  showCourseName?: boolean;
}

export default function QuizCard({
  quiz,
  index,
  className = "",
  showCourseName = true,
}: QuizCardProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const isGenerating = quiz.status === QuizStatusEnum.Generating;
  const isFailed = quiz.status === QuizStatusEnum.Failed;

  const rawTitle = quiz.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Quiz z") || rawTitle === quiz.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Quiz #${index}` : (rawTitle || (index !== undefined ? `Quiz #${index}` : "Quiz"));

  const isInProgress = quiz.lastAttemptStatus === QuizAttemptStatus.InProgress;

  return (
    <Card
      hoverEffect={!isGenerating}
      onClick={() => {
        if (!isGenerating) {
          navigate(`/quiz/${quiz.id}`, { state: { fromPath: location.pathname } });
        }
      }}
      className={`p-4 flex-row items-center justify-between gap-3.5 ${className}`}
    >
      <div className="min-w-0 flex-1 space-y-0.5">
        <Flex align="center" gap={2} className="min-w-0">
          <Heading level={4} size="sm" className="leading-snug truncate tile-title-scale">
            {displayTitle}
          </Heading>
          {isFailed && (
            <Badge variant="error" className="shrink-0">Błąd generowania (Ponawianie...)</Badge>
          )}
        </Flex>
        {showCourseName && quiz.courseName && (
          <Text size="xs" variant="muted" className="line-clamp-1 truncate">
            {quiz.courseName}
          </Text>
        )}
      </div>

      <Flex direction="col" align="end" justify="center" gap={0.5} className="shrink-0 text-right">
        {isGenerating ? (
          <Tooltip content={t("quizzes.btnGenerating")}>
            <div
              aria-label={t("quizzes.btnGenerating")}
              className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
            >
              <RefreshCw size={15} className="animate-spin" />
            </div>
          </Tooltip>
        ) : isFailed ? (
          <Tooltip content="Błąd generowania. Zadanie oczekuje na ponowienie...">
            <div
              className="w-8 h-8 rounded-full bg-destructive/15 text-destructive flex items-center justify-center shrink-0 border border-destructive/25"
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
    </Card>
  );
}
