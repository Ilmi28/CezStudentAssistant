import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import type { QuizDto } from "../types";
import { QuizStatusEnum, QuizAttemptStatus } from "../enums/quizEnums";
import Card from "./Card";
import { useCountdown, useQuiz } from "../hooks";

interface QuizCardProps {
  quiz: QuizDto;
  index?: number;
  className?: string;
  showCourseName?: boolean;
}

function CardAttemptCountdownBadge({
  expiresAt,
  percentage,
  onExpire,
}: {
  expiresAt: string;
  percentage: number | null;
  onExpire?: () => void;
}) {
  const { t } = useTranslation();
  const { formatted, isExpired, isTimeLow } = useCountdown(expiresAt, onExpire);

  if (isExpired) {
    return (
      <div className="flex flex-col items-end justify-center text-right">
        <span
          className={`text-base font-bold tabular-nums leading-tight ${
            percentage !== null && percentage >= 50
              ? "text-emerald-500 dark:text-emerald-400"
              : "text-amber-500 dark:text-amber-400"
          }`}
        >
          {percentage !== null ? `${percentage}%` : "0%"}
        </span>
        <span className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider">
          {t("quizDetails.status.completed")}
        </span>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-end justify-center text-right">
      <span
        className={`text-base font-bold tabular-nums leading-tight ${
          isTimeLow
            ? "text-rose-500 font-bold animate-pulse"
            : "text-amber-500 dark:text-amber-400"
        }`}
      >
        {formatted}
      </span>
      <span className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider">
        {t("quizDetails.status.inProgress", "W toku")}
      </span>
    </div>
  );
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
  const { refreshQuizzes } = useQuiz();
  const isGenerating = quiz.status === QuizStatusEnum.Generating;

  const rawTitle = quiz.displayName || quiz.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Quiz z") || rawTitle === quiz.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Quiz #${index}` : (rawTitle || (index !== undefined ? `Quiz #${index}` : "Quiz"));

  const isInProgress = quiz.lastAttemptStatus === QuizAttemptStatus.InProgress;

  const percentage =
    quiz.maxPoints && quiz.maxPoints > 0 && quiz.lastAttemptPoints !== null && quiz.lastAttemptPoints !== undefined
      ? Math.round((quiz.lastAttemptPoints / quiz.maxPoints) * 100)
      : null;

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
      <div className="min-w-0 flex-1">
        <h4 className="text-sm md:text-[15px] font-semibold text-foreground leading-snug line-clamp-1">
          {displayTitle}
        </h4>
        {showCourseName && quiz.courseName && (
          <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">
            {quiz.courseName}
          </p>
        )}
      </div>

      <div className="flex flex-col items-end justify-center gap-0.5 shrink-0 text-right">
        {isGenerating ? (
          <div
            title={t("quizzes.btnGenerating")}
            aria-label={t("quizzes.btnGenerating")}
            className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
          >
            <RefreshCw size={15} className="animate-spin" />
          </div>
        ) : (
          <>
            {/* If attempt is in progress */}
            {isInProgress && (
              quiz.lastAttemptExpiresAt ? (
                <CardAttemptCountdownBadge
                  expiresAt={quiz.lastAttemptExpiresAt}
                  percentage={percentage}
                  onExpire={() => refreshQuizzes().catch(() => {})}
                />
              ) : (
                <>
                  <span className="text-base font-bold text-amber-500 dark:text-amber-400 leading-tight">
                    {t("quizDetails.status.inProgress", "W toku")}
                  </span>
                  <span className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider">
                    {t("quizDetails.stats.noLimit", "Brak limitu")}
                  </span>
                </>
              )
            )}

            {/* Display Progress (0% - 100%) */}
            {!isInProgress && (
              <>
                <span
                  className={`text-base font-bold tabular-nums leading-tight ${
                    (quiz.progressPercentage ?? 0) >= 50
                      ? "text-emerald-500 dark:text-emerald-400"
                      : (quiz.progressPercentage ?? 0) > 0
                        ? "text-amber-500 dark:text-amber-400"
                        : "text-muted-foreground/70"
                  }`}
                >
                  {quiz.progressPercentage ?? 0}%
                </span>
                <span className="text-[10px] text-muted-foreground font-medium uppercase tracking-wider">
                  {t("quizDetails.stats.masteryIndex", "PROGRES")}
                </span>
              </>
            )}
          </>
        )}
      </div>
    </Card>
  );
}
