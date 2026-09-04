import { useTranslation } from "react-i18next";
import { Brain, Layers, Target } from "lucide-react";
import type { CourseDetailsDto } from "../../../types/courseTypes";
import { Card, Heading, Text, Flex, Grid } from "../../index";
import { getScoreColorClass } from "../../../utils/scoreUtils";

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
    <Card className="p-5 md:p-6 shadow-sm space-y-4">
      <Flex align="center" justify="between" wrap gap={3} className="pb-3 border-b border-border">
        <Flex align="center" gap={2.5}>
          <div className="w-9 h-9 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-center text-primary shrink-0">
            <Target size={18} strokeWidth={2.25} />
          </div>
          <div>
            <Heading level={2} size="sm" className="font-bold">
              {t("courseDetails.preparationTitle", "Poziom przygotowania do przedmiotu")}
            </Heading>
            <Text size="xs" variant="muted">
              {t("courseDetails.preparationSubtitle", "Wskaźnik wyliczony na podstawie wyników quizów i opanowanych fiszek")}
            </Text>
          </div>
        </Flex>

        <Flex align="baseline" gap={1.5}>
          {overallProgress !== null ? (
            <span className={`text-2xl font-black tabular-nums ${getScoreColorClass(overallProgress)}`}>
              {overallProgress}%
            </span>
          ) : (
            <Text size="2xl" variant="muted" className="font-black tabular-nums">
              -
            </Text>
          )}
        </Flex>
      </Flex>

      <div className="w-full h-3.5 bg-secondary rounded-full overflow-hidden border border-border/60">
        <div
          className="h-full bg-emerald-500 transition-all duration-500 ease-out"
          style={{
            width: `${overallProgress !== null ? Math.min(100, Math.max(0, overallProgress)) : 0}%`,
          }}
        />
      </div>

      <Grid cols={1} smCols={2} gap={0} className="divide-y sm:divide-y-0 sm:divide-x divide-border/50 pt-1">
        <Flex align="center" gap={3} className="py-2 sm:py-0 sm:px-4 first:pl-0">
          <div className="w-8 h-8 rounded-lg bg-sky-500/10 border border-sky-500/20 flex items-center justify-center text-sky-500 shrink-0">
            <Brain size={16} />
          </div>
          <div>
            <Text size="xs" variant="muted" className="font-medium">
              {t("courseDetails.quizProgressLabel", "Quizy")}
            </Text>
            <Text size="sm" variant="default" className="font-bold tabular-nums">
              {quizProgress !== null ? `${quizProgress}%` : "-"}
            </Text>
          </div>
        </Flex>

        <Flex align="center" gap={3} className="py-2 sm:py-0 sm:px-4 last:pr-0">
          <div className="w-8 h-8 rounded-lg bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-500 shrink-0">
            <Layers size={16} />
          </div>
          <div>
            <Text size="xs" variant="muted" className="font-medium">
              {t("courseDetails.flashcardsMasteredLabel", "Fiszki")}
            </Text>
            <Text size="sm" variant="default" className="font-bold tabular-nums">
              {flashcardProgress !== null ? `${flashcardProgress}%` : "-"}
            </Text>
          </div>
        </Flex>
      </Grid>
    </Card>
  );
}
