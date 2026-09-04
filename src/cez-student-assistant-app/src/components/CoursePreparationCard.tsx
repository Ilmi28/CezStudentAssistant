import { useTranslation } from "react-i18next";
import { Brain, Layers, Target } from "lucide-react";
import type { CourseDetailsDto } from "../types/courseTypes";
import { getScoreColorClass } from "../utils/scoreUtils";

interface CoursePreparationCardProps {
  course: CourseDetailsDto;
}

export default function CoursePreparationCard({
  course,
}: CoursePreparationCardProps) {
  const { t } = useTranslation();

  const overallProgress = course.preparationPercentage ?? null;
  const quizProgress = course.quizProgressPercentage ?? null;
  const flashcardProgress = course.flashcardProgressPercentage ?? null;

  return (
    <div className="bg-card rounded-xl border border-border p-5 md:p-6 shadow-sm space-y-4">
      {/* Top Title & Score */}
      <div className="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-border">
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
            <Target size={18} strokeWidth={2.25} />
          </div>
          <div>
            <h2 className="text-sm font-bold text-foreground">
              {t("courseDetails.preparationTitle", "Poziom przygotowania do przedmiotu")}
            </h2>
            <p className="text-xs text-muted-foreground">
              {t("courseDetails.preparationSubtitle", "Wskaźnik wyliczony na podstawie wyników quizów i opanowanych fiszek")}
            </p>
          </div>
        </div>

        <div className="flex items-baseline gap-1.5">
          {overallProgress !== null ? (
            <span className={`text-2xl font-black tabular-nums ${getScoreColorClass(overallProgress)}`}>
              {overallProgress}%
            </span>
          ) : (
            <span className="text-2xl font-black text-muted-foreground tabular-nums">
              -
            </span>
          )}
        </div>
      </div>

      {/* Single Green Progress Bar with Empty Space on Right */}
      <div className="w-full h-3.5 bg-secondary rounded-full overflow-hidden border border-border/60">
        <div
          className="h-full bg-emerald-500 transition-all duration-500 ease-out"
          style={{
            width: `${overallProgress !== null ? Math.min(100, Math.max(0, overallProgress)) : 0}%`,
          }}
        />
      </div>

      {/* 2 Metric Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 divide-y sm:divide-y-0 sm:divide-x divide-border/50 pt-1">
        {/* Metric 1: Quiz Progress */}
        <div className="flex items-center gap-3 py-2 sm:py-0 sm:px-4 first:pl-0">
          <div className="w-8 h-8 rounded-lg bg-sky-500/10 border border-sky-500/20 flex items-center justify-center text-sky-500 shrink-0">
            <Brain size={16} />
          </div>
          <div>
            <div className="text-[11px] font-medium text-muted-foreground">
              {t("courseDetails.quizProgressLabel", "Quizy")}
            </div>
            <div className="text-sm font-bold text-foreground tabular-nums">
              {quizProgress !== null ? (
                <>{quizProgress}%</>
              ) : (
                <span className="text-muted-foreground font-semibold">-</span>
              )}
            </div>
          </div>
        </div>

        {/* Metric 2: Flashcards Mastered */}
        <div className="flex items-center gap-3 py-2 sm:py-0 sm:px-4 last:pr-0">
          <div className="w-8 h-8 rounded-lg bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-500 shrink-0">
            <Layers size={16} />
          </div>
          <div>
            <div className="text-[11px] font-medium text-muted-foreground">
              {t("courseDetails.flashcardsMasteredLabel", "Fiszki")}
            </div>
            <div className="text-sm font-bold text-foreground tabular-nums">
              {flashcardProgress !== null ? (
                <>{flashcardProgress}%</>
              ) : (
                <span className="text-muted-foreground font-semibold">-</span>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
