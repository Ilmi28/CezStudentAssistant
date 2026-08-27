import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import type { QuizDto } from "../types";
import { QuizStatusEnum, QuizAttemptStatus } from "../enums/quizEnums";
import Card from "./Card";
import { useCountdown, useQuiz } from "../hooks";
import { getScoreColorClass } from "../utils/scoreUtils";

interface QuizCardProps {
  quiz: QuizDto;
  index?: number;
  className?: string;
  showCourseName?: boolean;
}

function CardInProgressBadge({
  expiresAt,
  onExpire,
}: {
  expiresAt?: string | null;
  onExpire?: () => void;
}) {
  const { t } = useTranslation();
  const { formatted, isExpired, isTimeLow } = useCountdown(expiresAt || null, onExpire);

  if (expiresAt && isExpired) {
    return null;
  }

  return (
    <span
      className={`px-2 py-0.5 rounded-md text-[10px] font-bold border shrink-0 inline-flex items-center gap-1.5 ${
        isTimeLow
          ? "bg-rose-500/15 text-rose-500 border-rose-500/30 animate-pulse"
          : "bg-amber-500/15 text-amber-500 dark:text-amber-400 border-amber-500/30"
      }`}
    >
      <span>{t("quizDetails.status.inProgress", "W toku")}</span>
      {expiresAt && formatted && (
        <span className="tabular-nums font-mono font-bold">• {formatted}</span>
      )}
    </span>
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
        <div className="flex items-center gap-2 min-w-0 flex-wrap">
          <h4 className="text-sm md:text-[15px] font-semibold text-foreground leading-snug truncate">
            {displayTitle}
          </h4>
          {isInProgress && (
            <CardInProgressBadge
              expiresAt={quiz.lastAttemptExpiresAt}
              onExpire={() => refreshQuizzes().catch(() => {})}
            />
          )}
          {isFailed && (
            <span className="text-[11px] font-medium text-destructive bg-destructive/10 px-2 py-0.5 rounded-full border border-destructive/20">
              Błąd generowania (Ponawianie...)
            </span>
          )}
        </div>
        {showCourseName && quiz.courseName && (
          <p className="text-xs text-muted-foreground line-clamp-1">
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
              className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(quiz.progressPercentage)}`}
            >
              {quiz.progressPercentage ?? 0}%
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
