import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Trophy, Brain, Activity } from "lucide-react";
import Card from "../components/Card";
import { useDashboard } from "../hooks";
import { getScoreColorClass } from "../utils/scoreUtils";

function formatScore(val: number): string {
  if (Number.isInteger(val)) return val.toString();
  return parseFloat(val.toFixed(2)).toString();
}

export default function HomePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { stats, recentActivity, loadingStats, loadingActivity } = useDashboard();

  const handleActivityClick = (item: { type: string; entityId: string; id: string }) => {
    if (item.type === "Quiz") {
      navigate(`/quiz/${item.entityId}`);
    } else {
      navigate(`/flashcards/${item.entityId}`);
    }
  };

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      {/* Unified Stats Card - Matched 100% to QuizDetailsPage / FlashcardDeckDetailsPage */}
      <Card className="p-4 sm:p-5">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3.5 xl:gap-4 divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          {/* Liczba przedmiotów */}
          <div className="flex items-center gap-3 pt-1 sm:pt-0">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Layers size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("home.stats.courses")}
              </span>
              <span className="text-xl font-bold text-foreground block truncate leading-tight">
                {loadingStats ? "..." : (stats?.courseCount ?? 0)}
              </span>
            </div>
          </div>

          {/* Liczba quizów */}
          <div className="flex items-center gap-3 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Trophy size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("home.stats.quizzes")}
              </span>
              <span className="text-xl font-bold text-foreground block truncate leading-tight">
                {loadingStats ? "..." : (stats?.quizCount ?? 0)}
              </span>
            </div>
          </div>

          {/* Liczba fiszek */}
          <div className="flex items-center gap-3 pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Brain size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <span className="text-[10.5px] uppercase tracking-wider text-muted-foreground font-semibold block whitespace-nowrap truncate">
                {t("home.stats.flashcards")}
              </span>
              <span className="text-xl font-bold text-foreground block truncate leading-tight">
                {loadingStats ? "..." : (stats?.flashcardCount ?? 0)}
              </span>
            </div>
          </div>
        </div>
      </Card>

      {/* Recent Activity Section */}
      <div>
        <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
            {t("home.recentActivity")}
          </h3>
        </div>

        {loadingActivity ? (
          <Card className="p-8 text-center shadow-sm">
            <p className="text-sm text-muted-foreground">{t("common.loading")}</p>
          </Card>
        ) : recentActivity.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <Activity size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <p className="text-sm text-muted-foreground">{t("home.noActivity")}</p>
              <p className="text-xs text-muted-foreground/60 mt-1">{t("home.noActivitySubtitle")}</p>
            </div>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {recentActivity.map((act) => {
              const isInProgress = act.status === "InProgress";
              const isQuiz = act.type === "Quiz";
              const displayTitle = act.title || (isQuiz ? "Quiz" : "Fiszki");

              return (
                <Card
                  key={act.id}
                  hoverEffect
                  onClick={() => handleActivityClick(act)}
                  className="p-4 flex-row items-center justify-between gap-3.5 cursor-pointer"
                >
                  <div className="min-w-0 flex-1 space-y-0.5">
                    <h4 className="text-sm md:text-[15px] font-semibold text-foreground leading-snug truncate">
                      {displayTitle}
                    </h4>
                    {act.courseName && (
                      <p className="text-xs text-muted-foreground line-clamp-1">
                        {act.courseName}
                      </p>
                    )}
                  </div>

                  {isInProgress ? (
                    <div className="flex flex-col items-end justify-center text-right shrink-0">
                      <span className="text-xs font-bold text-amber-500 dark:text-amber-400">
                        {t("quizDetails.status.inProgress", "W toku")}
                      </span>
                    </div>
                  ) : (
                    <div className="flex flex-col items-end justify-center gap-0.5 shrink-0 text-right">
                      <span
                        className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(
                          act.scorePercentage
                        )}`}
                      >
                        {act.scorePercentage ?? 0}%
                      </span>
                      {isQuiz ? (
                        <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
                          {formatScore(act.earnedPoints ?? 0)} / {formatScore(act.maxPoints ?? 0)}
                        </span>
                      ) : (
                        <span className="text-[11px] text-muted-foreground font-medium tabular-nums">
                          {act.masteredCount ?? 0} / {act.totalCount ?? 0}
                        </span>
                      )}
                    </div>
                  )}
                </Card>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
