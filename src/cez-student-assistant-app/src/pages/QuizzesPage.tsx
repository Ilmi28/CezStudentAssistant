import { useTranslation } from "react-i18next";
import { Brain } from "lucide-react";
import type { QuizDto } from "../services";
import Card from "../components/Card";
import QuizCard from "../components/QuizCard";

interface QuizzesPageProps {
  quizzes: QuizDto[];
}

export default function QuizzesPage({ quizzes }: QuizzesPageProps) {
  const { t } = useTranslation();

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
            {t("quizzes.title")}
          </h3>
        </div>

        {quizzes.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <Brain size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <p className="text-sm text-muted-foreground">{t("quizzes.noQuizzes")}</p>
              <p className="text-xs text-muted-foreground/60 mt-1">{t("quizzes.noQuizzesSubtitle")}</p>
            </div>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {quizzes.map((q, idx) => (
              <QuizCard key={q.id} quiz={q} index={idx + 1} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
