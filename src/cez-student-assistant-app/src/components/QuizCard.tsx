import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Play, RefreshCw } from "lucide-react";
import type { QuizDto } from "../types";
import { QuizStatusEnum } from "../enums/quizEnums";
import Card from "./Card";

interface QuizCardProps {
  quiz: QuizDto;
  index?: number;
  className?: string;
  showCourseName?: boolean;
}

export default function QuizCard({
  quiz,
  index,
  className = "",
  showCourseName = true,
}: QuizCardProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const isGenerating = quiz.status === QuizStatusEnum.Generating;

  const rawTitle = quiz.displayName || quiz.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Quiz z") || rawTitle === quiz.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Quiz #${index}` : (rawTitle || (index !== undefined ? `Quiz #${index}` : "Quiz"));

  return (
    <Card
      hoverEffect={!isGenerating}
      onClick={() => {
        if (!isGenerating) {
          navigate(`/quiz/${quiz.id}`);
        }
      }}
      className={`p-4 flex-row items-center justify-between gap-3.5 ${className}`}
    >
      <div className="min-w-0 flex-1">
        <h4 className="text-sm md:text-[15px] font-semibold text-foreground leading-snug line-clamp-1">
          {displayTitle}
        </h4>
        {showCourseName && quiz.courseName && (
          <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">
            {quiz.courseName}
          </p>
        )}
      </div>

      {isGenerating ? (
        <button
          type="button"
          disabled
          title={t("quizzes.btnGenerating")}
          aria-label={t("quizzes.btnGenerating")}
          className="w-9 h-9 rounded-full bg-primary/20 text-primary flex items-center justify-center shrink-0 cursor-not-allowed border border-primary/30"
        >
          <RefreshCw size={18} className="animate-spin" />
        </button>
      ) : (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/quiz/${quiz.id}`);
          }}
          title={t("home.startQuiz")}
          aria-label={t("home.startQuiz")}
          className="w-9 h-9 rounded-full bg-primary hover:bg-primary/90 active:scale-95 text-white flex items-center justify-center shrink-0 shadow-sm hover:shadow-md transition-all cursor-pointer"
        >
          <Play size={20} strokeWidth={2.25} className="ml-0.5" />
        </button>
      )}
    </Card>
  );
}
