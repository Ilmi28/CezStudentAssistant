import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Brain } from "lucide-react";
import type { QuizDto } from "../services";
import Card from "../components/Card";
import { PrimaryButton } from "../components/Button";

interface QuizzesPageProps {
  quizzes: QuizDto[];
}

export default function QuizzesPage({ quizzes }: QuizzesPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
            {t("quizzes.title")}
          </h3>
          <span className="text-xs text-muted-foreground">{t("quizzes.subtitle")}</span>
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
            {quizzes.map((q) => (
              <Card key={q.id} hoverEffect>
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-[10px] bg-primary/10 text-primary font-semibold px-2 py-0.5 rounded uppercase">
                      QUIZ AI
                    </span>
                    <span className="text-xs text-muted-foreground/40 truncate max-w-[120px]">{q.id.substring(0, 8)}</span>
                  </div>
                  <h4 className="text-sm font-bold text-foreground mb-1 line-clamp-1">
                    {q.displayName || q.name}
                  </h4>
                  <p className="text-[11px] text-muted-foreground mb-4 line-clamp-1">
                    {q.courseName}
                  </p>
                </div>
                <PrimaryButton
                  onClick={() => navigate(`/quiz/${q.id}`)}
                  icon={<Brain size={13} />}
                >
                  {t("home.startQuiz")}
                </PrimaryButton>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
