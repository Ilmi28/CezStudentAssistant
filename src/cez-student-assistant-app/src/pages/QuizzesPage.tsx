import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Brain, ArrowRight } from "lucide-react";
import type { QuizDto } from "../services/api";

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
          <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground">
            {t("quizzes.title")}
          </h3>
          <span className="text-[10px] text-muted-foreground font-mono">{t("quizzes.subtitle")}</span>
        </div>

        {quizzes.length === 0 ? (
          <div className="bg-card rounded-lg border border-border p-8 text-center shadow-sm">
            <Brain size={32} className="mx-auto text-muted-foreground/30 mb-2" />
            <p className="text-[13px] text-muted-foreground">{t("quizzes.noQuizzes")}</p>
            <p className="text-[11px] text-muted-foreground/60 mt-1">{t("quizzes.noQuizzesSubtitle")}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {quizzes.map((q) => (
              <div
                key={q.id}
                className="bg-card rounded-lg border border-border p-5 shadow-sm hover:border-primary/45 hover:shadow transition-all flex flex-col justify-between"
              >
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-[9px] bg-primary/10 text-primary font-semibold px-2 py-0.5 rounded font-mono uppercase">
                      QUIZ AI
                    </span>
                    <span className="text-[10px] text-muted-foreground/40 font-mono truncate max-w-[120px]">{q.id.substring(0, 8)}</span>
                  </div>
                  <h4 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold text-foreground mb-1 line-clamp-1">
                    {q.displayName || q.name}
                  </h4>
                  <p className="text-[11px] text-muted-foreground mb-4 line-clamp-1">
                    {q.courseName}
                  </p>
                </div>
                <button
                  onClick={() => navigate(`/quiz/${q.id}`)}
                  className="w-full bg-primary hover:bg-primary/90 text-white py-2 rounded text-[12px] font-medium transition-colors flex items-center justify-center gap-1.5 cursor-pointer shadow-sm"
                >
                  <Brain size={13} />
                  {t("home.startQuiz")}
                  <ArrowRight size={12} />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
