import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Brain, RefreshCw } from "lucide-react";
import type { QuizDto } from "../types";
import { QuizStatusEnum } from "../enums/quizEnums";
import Card from "./Card";
import { PrimaryButton } from "./Button";

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
  showCourseName = true
}: QuizCardProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const isGenerating = quiz.status === QuizStatusEnum.Generating;

  const rawTitle = quiz.displayName || quiz.name;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Quiz z") || rawTitle === quiz.courseName;
  const displayTitle = index !== undefined && isGenericTitle ? `Quiz #${index}` : (rawTitle || (index !== undefined ? `Quiz #${index}` : "Quiz"));

  return (
    <Card hoverEffect={!isGenerating} className={`p-4 flex flex-col justify-between ${className}`}>
      <div>
        <h4 className="text-xs font-bold text-foreground mb-1 line-clamp-1">
          {displayTitle}
        </h4>
        {showCourseName && quiz.courseName && (
          <p className="text-[11px] text-muted-foreground mb-3 line-clamp-1">
            {quiz.courseName}
          </p>
        )}
      </div>

      {isGenerating ? (
        <PrimaryButton
          disabled
          icon={<RefreshCw size={13} className="animate-spin" />}
          className="w-full text-xs py-2 opacity-75 cursor-not-allowed mt-3"
        >
          {t("quizzes.btnGenerating")}
        </PrimaryButton>
      ) : (
        <PrimaryButton
          onClick={() => navigate(`/quiz/${quiz.id}`)}
          icon={<Brain size={13} />}
          className="w-full text-xs py-2 mt-3"
        >
          {t("home.startQuiz")}
        </PrimaryButton>
      )}
    </Card>
  );
}
