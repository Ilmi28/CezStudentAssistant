import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Brain } from "lucide-react";
import { Card, QuizCard, Flex, Grid, Heading, Text } from "../components";
import { useQuiz } from "../hooks";

export default function QuizzesPage() {
  const { t } = useTranslation();
  const { quizzes, refreshQuizzes } = useQuiz();

  useEffect(() => {
    refreshQuizzes();
  }, []);

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <Flex align="center" justify="between" className="mb-4 border-b border-border pb-2">
          <Heading level={3} size="sm" uppercase className="tracking-wider">
            {t("quizzes.title")}
          </Heading>
        </Flex>

        {quizzes.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <Brain size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <Text size="sm" variant="muted">{t("quizzes.noQuizzes")}</Text>
              <Text size="xs" variant="subtle" className="mt-1">{t("quizzes.noQuizzesSubtitle")}</Text>
            </div>
          </Card>
        ) : (
          <Grid cols={1} mdCols={2} gap={4}>
            {quizzes.map((q, idx) => (
              <QuizCard key={q.id} quiz={q} index={idx + 1} />
            ))}
          </Grid>
        )}
      </div>
    </div>
  );
}
