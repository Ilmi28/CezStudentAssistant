import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
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

      {isGenerating && (
        <div
          title={t("quizzes.btnGenerating")}
          aria-label={t("quizzes.btnGenerating")}
          className="w-8 h-8 rounded-full bg-primary/15 text-primary flex items-center justify-center shrink-0 border border-primary/25"
        >
          <RefreshCw size={15} className="animate-spin" />
        </div>
      )}
    </Card>
  );
}
