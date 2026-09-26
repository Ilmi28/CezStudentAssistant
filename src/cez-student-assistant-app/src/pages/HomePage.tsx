import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Trophy, Brain, MessageSquare, Activity } from "lucide-react";
import { Card, Heading, Text, Flex, Grid } from "../components";
import { useDashboard } from "../hooks";
import { getScoreColorClass } from "../utils/scoreUtils";
import { ActivityTypeEnum } from "../enums/activityEnums";
import type { RecentActivityDto } from "../types";

function formatScore(val: number): string {
  if (Number.isInteger(val)) return val.toString();
  return parseFloat(val.toFixed(2)).toString();
}

export default function HomePage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const { stats, recentActivity, loadingStats, loadingActivity } = useDashboard();

  const handleActivityClick = (item: RecentActivityDto) => {
    if (item.type === ActivityTypeEnum.Quiz) {
      navigate(`/quiz/${item.entityId}`, { state: { fromPath: location.pathname } });
    } else if (item.type === ActivityTypeEnum.Flashcard) {
      navigate(`/flashcards/${item.entityId}`, { state: { fromPath: location.pathname } });
    } else if (item.type === ActivityTypeEnum.Chat) {
      navigate(`/chats/${item.entityId}`, { state: { fromPath: location.pathname } });
    }
  };

  return (
    <div className="space-y-6 sm:space-y-8 animate-in fade-in duration-300">
      {/* Dashboard Quick Stats */}
      <Grid cols={2} lgCols={4} gap={3} className="sm:gap-4">
        <Card className="p-3.5 sm:p-4 hover:border-primary/40 transition-colors">
          <Flex align="center" gap={3}>
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Layers size={18} className="sm:w-5 sm:h-5" strokeWidth={2} />
            </div>
            <div className="min-w-0 flex-1">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block truncate tracking-wider text-[10px] sm:text-xs">
                {t("home.stats.courses")}
              </Text>
              <Text size="lg" variant="default" className="font-bold block truncate leading-tight sm:text-xl">
                {loadingStats ? "..." : (stats?.courseCount ?? 0)}
              </Text>
            </div>
          </Flex>
        </Card>

        <Card className="p-3.5 sm:p-4 hover:border-primary/40 transition-colors">
          <Flex align="center" gap={3}>
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Trophy size={18} className="sm:w-5 sm:h-5" strokeWidth={2} />
            </div>
            <div className="min-w-0 flex-1">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block truncate tracking-wider text-[10px] sm:text-xs">
                {t("home.stats.quizzes")}
              </Text>
              <Text size="lg" variant="default" className="font-bold block truncate leading-tight sm:text-xl">
                {loadingStats ? "..." : (stats?.quizCount ?? 0)}
              </Text>
            </div>
          </Flex>
        </Card>

        <Card className="p-3.5 sm:p-4 hover:border-primary/40 transition-colors">
          <Flex align="center" gap={3}>
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <Brain size={18} className="sm:w-5 sm:h-5" strokeWidth={2} />
            </div>
            <div className="min-w-0 flex-1">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block truncate tracking-wider text-[10px] sm:text-xs">
                {t("home.stats.flashcards")}
              </Text>
              <Text size="lg" variant="default" className="font-bold block truncate leading-tight sm:text-xl">
                {loadingStats ? "..." : (stats?.flashcardCount ?? 0)}
              </Text>
            </div>
          </Flex>
        </Card>

        <Card className="p-3.5 sm:p-4 hover:border-primary/40 transition-colors">
          <Flex align="center" gap={3}>
            <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-secondary/80 border border-border flex items-center justify-center text-foreground shrink-0">
              <MessageSquare size={18} className="sm:w-5 sm:h-5" strokeWidth={2} />
            </div>
            <div className="min-w-0 flex-1">
              <Text size="xs" variant="subtle" uppercase className="font-semibold block truncate tracking-wider text-[10px] sm:text-xs">
                {t("home.stats.chats")}
              </Text>
              <Text size="lg" variant="default" className="font-bold block truncate leading-tight sm:text-xl">
                {loadingStats ? "..." : (stats?.chatCount ?? 0)}
              </Text>
            </div>
          </Flex>
        </Card>
      </Grid>

      {/* Recent Activity Section */}
      <div>
        <Flex align="center" justify="between" className="mb-3.5 sm:mb-4 border-b border-border pb-2">
          <Heading level={3} size="sm" uppercase className="tracking-wider text-xs sm:text-sm">
            {t("home.recentActivity")}
          </Heading>
        </Flex>

        {loadingActivity ? (
          <Card className="p-6 sm:p-8 text-center shadow-sm">
            <Text size="sm" variant="muted">{t("common.loading")}</Text>
          </Card>
        ) : recentActivity.length === 0 ? (
          <Card className="p-6 sm:p-8 text-center shadow-sm">
            <div>
              <Activity size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <Text size="sm" variant="muted">{t("home.noActivity")}</Text>
              <Text size="xs" variant="subtle" className="mt-1">{t("home.noActivitySubtitle")}</Text>
            </div>
          </Card>
        ) : (
          <div className="space-y-2.5">
            {recentActivity.map((act) => {
              const isInProgress = act.status === "InProgress";
              const isQuiz = act.type === ActivityTypeEnum.Quiz;
              const isChat = act.type === ActivityTypeEnum.Chat;
              const displayTitle = act.title || (isQuiz ? "Quiz" : isChat ? "Czat" : "Fiszki");

              return (
                <Card
                  key={act.id}
                  hoverEffect
                  onClick={() => handleActivityClick(act)}
                  className="p-3 sm:p-4 flex-row items-center justify-between gap-3 sm:gap-4 cursor-pointer"
                >
                  <div className="min-w-0 flex-1 space-y-0.5">
                    <Heading level={4} size="sm" className="line-clamp-1 truncate text-xs sm:text-sm leading-snug tile-title-scale" title={displayTitle}>
                      {displayTitle}
                    </Heading>
                    {act.courseName && (
                      <Text size="xs" variant="muted" className="line-clamp-1 truncate text-[11px] sm:text-xs">
                        {act.courseName}
                      </Text>
                    )}
                  </div>

                  <Flex direction="col" align="end" justify="center" gap={0.5} className="shrink-0 text-right">
                    <span className="text-[10px] sm:text-xs font-bold text-primary uppercase tracking-wider">
                      {isQuiz
                        ? t("home.badgeQuiz", "QUIZ")
                        : isChat
                        ? t("home.badgeChat", "CZAT")
                        : t("home.badgeFlashcard", "FISZKI")}
                    </span>

                    {isInProgress ? (
                      <span className="text-[10px] sm:text-xs font-bold text-amber-500 dark:text-amber-400 uppercase tracking-wider">
                        {t("quizDetails.status.inProgress", "W TOKU")}
                      </span>
                    ) : isChat ? (
                      <Text size="xs" variant="muted" className="font-medium tabular-nums text-[11px] sm:text-xs">
                        {t("home.messagesCount", { count: act.totalCount ?? 0 })}
                      </Text>
                    ) : isQuiz ? (
                      <Text size="xs" variant="muted" className="font-medium tabular-nums text-[11px] sm:text-xs">
                        <span className={`font-bold ${getScoreColorClass(act.scorePercentage)}`}>
                          {act.scorePercentage ?? 0}%
                        </span>
                        {" • "}
                        {formatScore(act.earnedPoints ?? 0)} / {formatScore(act.maxPoints ?? 0)}
                      </Text>
                    ) : (
                      <Text size="xs" variant="muted" className="font-medium tabular-nums text-[11px] sm:text-xs">
                        <span className={`font-bold ${getScoreColorClass(act.scorePercentage)}`}>
                          {act.scorePercentage ?? 0}%
                        </span>
                        {" • "}
                        {act.masteredCount ?? 0} / {act.totalCount ?? 0}
                      </Text>
                    )}
                  </Flex>
                </Card>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
