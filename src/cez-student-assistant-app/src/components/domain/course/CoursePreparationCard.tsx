import { useTranslation } from "react-i18next";
import type { CourseDetailsDto } from "../../../types/courseTypes";
import { Card, Heading, Text, Flex, Grid } from "../../index";

interface CoursePreparationCardProps {
  course: CourseDetailsDto;
}

export default function CoursePreparationCard({
  course,
}: CoursePreparationCardProps) {
  const { t } = useTranslation();

  const overallProgress = course.preparationPercentage ?? 0;
  const quizProgress = course.quizProgressPercentage ?? 0;
  const flashcardProgress = course.flashcardProgressPercentage ?? 0;

  return (
    <Card className="p-5 md:p-6 shadow-sm space-y-4">
      <Flex align="center" justify="between" wrap gap={3} className="pb-3 border-b border-border">
        <div>
          <Heading level={2} size="sm" className="font-bold">
            {t("courseDetails.preparationTitle", "Poziom przygotowania do przedmiotu")}
          </Heading>
          <Text size="xs" variant="muted">
            {t("courseDetails.preparationSubtitle", "Wskaźnik wyliczony na podstawie wyników quizów i opanowanych fiszek")}
          </Text>
        </div>

        <Flex align="baseline" gap={1.5}>
          <span className="text-2xl font-black tabular-nums text-foreground">
            {overallProgress}%
          </span>
        </Flex>
      </Flex>

      <div className="w-full h-3 bg-secondary rounded-full overflow-hidden border border-border/40">
        <div
          className="h-full bg-primary transition-all duration-500 ease-out"
          style={{
            width: `${Math.min(100, Math.max(0, overallProgress))}%`,
          }}
        />
      </div>

      <Grid cols={2} gap={4} className="pt-1">
        <div className="space-y-0.5">
          <Text size="xs" variant="muted" className="font-medium">
            {t("courseDetails.quizProgressLabel", "Quizy")}
          </Text>
          <div className="text-base font-bold tabular-nums text-foreground">
            {quizProgress}%
          </div>
        </div>

        <div className="space-y-0.5">
          <Text size="xs" variant="muted" className="font-medium">
            {t("courseDetails.flashcardsMasteredLabel", "Fiszki")}
          </Text>
          <div className="text-base font-bold tabular-nums text-foreground">
            {flashcardProgress}%
          </div>
        </div>
      </Grid>
    </Card>
  );
}
