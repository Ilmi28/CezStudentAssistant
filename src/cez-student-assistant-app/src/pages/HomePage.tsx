import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Trophy, Zap, Target, Brain, ArrowRight } from "lucide-react";
import type { CourseDto, QuizDto } from "../services/api";

interface HomePageProps {
  courses: CourseDto[];
  quizzes: QuizDto[];
}

export default function HomePage({ courses, quizzes }: HomePageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const latestQuizzes = quizzes.slice(0, 4);

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      {/* Stats widgets */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {[
          { label: t("home.stats.courses"), value: `${courses.length}`, icon: Layers, note: t("home.stats.coursesNote") },
          { label: t("home.stats.quizzes"), value: `${quizzes.length}`, icon: Trophy, note: t("home.stats.quizzesNote") },
          { label: t("home.stats.status"), value: t("home.stats.statusValue"), icon: Zap, note: t("home.stats.statusNote") },
          { label: t("home.stats.average"), value: "4.5", icon: Target, note: t("home.stats.averageNote") }
        ].map((s, i) => (
          <div key={i} className={`bg-card rounded-lg border border-border px-5 py-4 shadow-sm ${i === 0 ? "border-l-4 border-l-primary" : ""}`}>
            <div className="flex items-center gap-2 mb-2">
              <s.icon size={13} className="text-primary" />
              <span className="text-[10px] uppercase tracking-wider text-muted-foreground font-semibold">{s.label}</span>
            </div>
            <div className="text-[24px] font-bold text-foreground" style={{ fontFamily: "Roboto Slab, serif" }}>{s.value}</div>
            <div className="text-[11px] text-muted-foreground/60 mt-1">{s.note}</div>
          </div>
        ))}
      </div>

      {/* Latest Quizzes Preview */}
      <div>
        <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
          <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground">
            {t("home.latestQuizzes")}
          </h3>
          <span className="text-[10px] text-muted-foreground font-mono">{t("home.latestQuizzesTag")}</span>
        </div>

        {quizzes.length === 0 ? (
          <div className="bg-card rounded-lg border border-border p-8 text-center shadow-sm">
            <Brain size={32} className="mx-auto text-muted-foreground/30 mb-2" />
            <p className="text-[13px] text-muted-foreground">{t("home.noQuizzes")}</p>
            <p className="text-[11px] text-muted-foreground/60 mt-1">{t("home.noQuizzesSubtitle")}</p>
          </div>
        ) : (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {latestQuizzes.map((q) => (
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

            {quizzes.length > 4 && (
              <div className="flex justify-center pt-2">
                <button
                  onClick={() => navigate("/quizzes")}
                  className="inline-flex items-center gap-1.5 text-[12px] text-primary hover:underline font-semibold cursor-pointer"
                >
                  {t("home.viewAllQuizzes", { count: quizzes.length })} <ArrowRight size={13} />
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
