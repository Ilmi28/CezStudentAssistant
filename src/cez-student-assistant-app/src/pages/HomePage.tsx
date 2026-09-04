import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Trophy, Brain, Activity } from "lucide-react";
import { Card, Heading, Text, Flex, Grid } from "../components";
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
      <Card className="p-4 sm:p-5">
        <Grid cols={1} smCols={3} gap={4} className="divide-y sm:divide-y-0 sm:divide-x divide-border/50">
          <Flex align="center" gap={3} className="pt-1 sm:pt-0">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Layers size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block whitespace-nowrap truncate tracking-wider">
                {t("home.stats.courses")}
              </Text>
              <Text size="xl" variant="default" className="font-bold block truncate leading-tight">
                {loadingStats ? "..." : (stats?.courseCount ?? 0)}
              </Text>
            </div>
          </Flex>

          <Flex align="center" gap={3} className="pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Trophy size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block whitespace-nowrap truncate tracking-wider">
                {t("home.stats.quizzes")}
              </Text>
              <Text size="xl" variant="default" className="font-bold block truncate leading-tight">
                {loadingStats ? "..." : (stats?.quizCount ?? 0)}
              </Text>
            </div>
          </Flex>

          <Flex align="center" gap={3} className="pt-3 sm:pt-0 sm:pl-3 xl:pl-4">
            <div className="w-10 h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Brain size={20} strokeWidth={2} />
            </div>
            <div className="min-w-0">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block whitespace-nowrap truncate tracking-wider">
                {t("home.stats.flashcards")}
              </Text>
              <Text size="xl" variant="default" className="font-bold block truncate leading-tight">
                {loadingStats ? "..." : (stats?.flashcardCount ?? 0)}
              </Text>
            </div>
          </Flex>
        </Grid>
      </Card>

      <div>
        <Flex align="center" justify="between" className="mb-4 border-b border-border pb-2">
          <Heading level={3} size="sm" uppercase className="tracking-wider">
            {t("home.recentActivity")}
          </Heading>
        </Flex>

        {loadingActivity ? (
          <Card className="p-8 text-center shadow-sm">
            <Text size="sm" variant="muted">{t("common.loading")}</Text>
          </Card>
        ) : recentActivity.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <Activity size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <Text size="sm" variant="muted">{t("home.noActivity")}</Text>
              <Text size="xs" variant="subtle" className="mt-1">{t("home.noActivitySubtitle")}</Text>
            </div>
          </Card>
        ) : (
          <Grid cols={1} mdCols={2} gap={4}>
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
                    <Heading level={4} size="sm" className="truncate leading-snug">
                      {displayTitle}
                    </Heading>
                    {(act.courseName || isInProgress) && (
                      <Flex align="center" gap={1.5} className="min-w-0">
                        {act.courseName && (
                          <Text size="xs" variant="muted" className="line-clamp-1 truncate">
                            {act.courseName}
                          </Text>
                        )}
                        {act.courseName && isInProgress && (
                          <span className="text-muted-foreground/60 text-xs shrink-0">•</span>
                        )}
                        {isInProgress && (
                          <span className="shrink-0 text-[11px] font-bold text-amber-500 dark:text-amber-400 uppercase tracking-wider">
                            {t("quizDetails.status.inProgress", "W TOKU")}
                          </span>
                        )}
                      </Flex>
                    )}
                  </div>

                  <Flex direction="col" align="end" justify="center" gap={1} className="shrink-0 text-right">
                    <span
                      className={`text-base font-bold tabular-nums leading-tight ${getScoreColorClass(
                        act.scorePercentage
                      )}`}
                    >
                      {act.scorePercentage ?? 0}%
                    </span>
                    {isQuiz ? (
                      <Text size="xs" variant="muted" className="font-medium tabular-nums">
                        {formatScore(act.earnedPoints ?? 0)} / {formatScore(act.maxPoints ?? 0)}
                      </Text>
                    ) : (
                      <Text size="xs" variant="muted" className="font-medium tabular-nums">
                        {act.masteredCount ?? 0} / {act.totalCount ?? 0}
                      </Text>
                    )}
                  </Flex>
                </Card>
              );
            })}
          </Grid>
        )}
      </div>
    </div>
  );
}
