import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { CheckCircle, Check, ChevronLeft, ChevronRight, Trophy, RefreshCw } from "lucide-react";
import { quizService, QuestionDifficulty, QuestionType, type QuizAttemptDetailsDto, type QuestionDto } from "../services";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import ConfirmModal from "../components/ConfirmModal";

interface QuizSolverPageProps {
  setError: (msg: string) => void;
}

export default function QuizSolverPage({
  setError
}: QuizSolverPageProps) {
  const { attemptId, id } = useParams<{ attemptId?: string; id?: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const [attemptDetails, setAttemptDetails] = useState<QuizAttemptDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [isReviewMode, setIsReviewMode] = useState(false);
  const [selectedAnswers, setSelectedAnswers] = useState<Record<string, string[]>>({});
  const [finished, setFinished] = useState(false);
  const [finalScore, setFinalScore] = useState(0);
  const [answersSummary, setAnswersSummary] = useState<{
    questionId: string;
    isCorrect: boolean;
    chosenOptionIds: string[];
  }[]>([]);
  const [showSubmitModal, setShowSubmitModal] = useState(false);

  useEffect(() => {
    loadAttempt();
  }, [attemptId, id]);

  const loadAttempt = async () => {
    const targetAttemptId = attemptId || (location.state as { initialAttempt?: QuizAttemptDetailsDto })?.initialAttempt?.attemptId;
    const targetQuizId = id;

    setLoading(true);
    try {
      let details: QuizAttemptDetailsDto;

      if ((location.state as { initialAttempt?: QuizAttemptDetailsDto })?.initialAttempt) {
        details = (location.state as { initialAttempt: QuizAttemptDetailsDto }).initialAttempt;
      } else if (targetAttemptId) {
        details = await quizService.getQuizAttempt(targetAttemptId);
      } else if (targetQuizId) {
        details = await quizService.startQuiz(targetQuizId);
      } else {
        navigate("/quizzes");
        return;
      }

      setAttemptDetails(details);

      const initialAnswers: Record<string, string[]> = {};
      (details.answers || []).forEach(ans => {
        initialAnswers[ans.questionId] = ans.selectedOptionIds;
      });
      setSelectedAnswers(initialAnswers);

      const isCompleted = !details.isPending;

      if (isCompleted) {
        const summary = details.questions.map(q => {
          const userSelected = initialAnswers[q.id] || [];
          const correctOptions = q.options.filter(o => o.isCorrect).map(o => o.id);
          const isCorrect = userSelected.length === correctOptions.length &&
            userSelected.every(optId => correctOptions.includes(optId));
          return {
            questionId: q.id,
            isCorrect,
            chosenOptionIds: userSelected
          };
        });

        const totalScore = details.points ?? summary.reduce((sum, item) => {
          const q = details.questions.find(quest => quest.id === item.questionId);
          return item.isCorrect && q ? sum + Number(q.points) : sum;
        }, 0);

        setAnswersSummary(summary);
        setFinalScore(Number(totalScore));
        setFinished(true);
      } else {
        const firstUnansweredIndex = details.questions.findIndex(
          q => !initialAnswers[q.id] || initialAnswers[q.id].length === 0
        );
        setCurrentQuestionIndex(firstUnansweredIndex !== -1 ? firstUnansweredIndex : 0);
        setIsReviewMode(false);
        setFinished(false);
      }
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to load quiz attempt:", err);
      setError(t("quizSolver.loadingQuiz"));
      navigate("/quizzes");
    } finally {
      setLoading(false);
    }
  };

  const getTypeBadge = (type?: QuestionType) => {
    const isMultiple = type === QuestionType.MultipleChoice;
    return (
      <span className="text-[10px] bg-muted text-muted-foreground font-semibold px-2 py-0.5 rounded border border-border">
        {isMultiple ? t("quizSolver.type.multiple") : t("quizSolver.type.single")}
      </span>
    );
  };

  const getDifficultyBadge = (difficulty?: QuestionDifficulty) => {
    switch (difficulty) {
      case QuestionDifficulty.Easy:
        return (
          <span className="text-[10px] bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.easy")}
          </span>
        );
      case QuestionDifficulty.Hard:
        return (
          <span className="text-[10px] bg-rose-500/15 text-rose-600 dark:text-rose-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.hard")}
          </span>
        );
      case QuestionDifficulty.Medium:
      default:
        return (
          <span className="text-[10px] bg-amber-500/15 text-amber-600 dark:text-amber-400 font-bold px-2 py-0.5 rounded uppercase">
            {t("quizSolver.difficulty.medium")}
          </span>
        );
    }
  };

  const handleToggleOption = async (questionId: string, optionId: string, isMultipleChoice: boolean) => {
    if (finished || !attemptDetails) return;

    const currentSelected = selectedAnswers[questionId] || [];
    let updatedSelected: string[];

    if (isMultipleChoice) {
      updatedSelected = currentSelected.includes(optionId)
        ? currentSelected.filter(id => id !== optionId)
        : [...currentSelected, optionId];
    } else {
      updatedSelected = [optionId];
    }

    setSelectedAnswers(prev => ({
      ...prev,
      [questionId]: updatedSelected
    }));

    if (updatedSelected.length > 0) {
      try {
        await quizService.submitAnswer(
          attemptDetails.attemptId,
          questionId,
          updatedSelected
        );
      } catch (err) {
        console.warn("[QuizSolverPage] Failed to save answer:", err);
      }
    }
  };

  const handleConfirmSubmit = async () => {
    if (!attemptDetails) return;

    try {
      await quizService.completeQuizAttempt(attemptDetails.attemptId);
      const updatedDetails = await quizService.getQuizAttempt(attemptDetails.attemptId);
      setAttemptDetails(updatedDetails);

      const summary = updatedDetails.questions.map(q => {
        const userSelected = selectedAnswers[q.id] || [];
        const correctOptions = q.options.filter(o => o.isCorrect).map(o => o.id);
        const isCorrect = userSelected.length === correctOptions.length &&
          userSelected.every(optId => correctOptions.includes(optId));
        return {
          questionId: q.id,
          isCorrect,
          chosenOptionIds: userSelected
        };
      });

      const totalScore = updatedDetails.points ?? summary.reduce((sum, item) => {
        const q = updatedDetails.questions.find(quest => quest.id === item.questionId);
        return item.isCorrect && q ? sum + Number(q.points) : sum;
      }, 0);

      setAnswersSummary(summary);
      setFinalScore(Number(totalScore));
      setFinished(true);
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to complete quiz attempt:", err);
      setError(t("quizSolver.loadingQuiz"));
    }
  };

  const handleRetry = async () => {
    if (!attemptDetails) return;
    try {
      const newAttempt = await quizService.startQuiz(attemptDetails.quizId);
      setAttemptDetails(newAttempt);
      setSelectedAnswers({});
      setCurrentQuestionIndex(0);
      setIsReviewMode(false);
      setAnswersSummary([]);
      setFinalScore(0);
      setFinished(false);
      navigate(`/quiz/attempt/${newAttempt.attemptId}`, { replace: true });
    } catch (err) {
      console.warn("[QuizSolverPage] Failed to retry quiz:", err);
      navigate(`/quiz/${attemptDetails.quizId}`);
    }
  };

  const handleCancel = () => {
    if (attemptDetails?.quizId) {
      navigate(`/quiz/${attemptDetails.quizId}`);
    } else {
      navigate("/quizzes");
    }
  };

  if (loading || !attemptDetails) {
    return (
      <div className="flex justify-center py-12">
        <div className="flex items-center gap-3 bg-card px-6 py-4 rounded-lg border border-border shadow-sm">
          <RefreshCw size={18} className="animate-spin text-primary" />
          <span className="text-[13px] font-medium text-muted-foreground">{t("quizSolver.loadingQuiz")}</span>
        </div>
      </div>
    );
  }

  const qCount = attemptDetails.questions.length;
  const answeredCount = attemptDetails.questions.filter(
    q => (selectedAnswers[q.id]?.length || 0) > 0
  ).length;
  const isAllAnswered = answeredCount === qCount && qCount > 0;

  if (finished) {
    const totalPointsMax = attemptDetails.questions.reduce((sum, q) => sum + Number(q.points), 0);
    return (
      <div className="max-w-2xl mx-auto bg-card rounded-xl border border-border p-8 text-center shadow-sm space-y-6 animate-in fade-in duration-300">
        <div className="inline-flex w-16 h-16 rounded-full bg-primary/10 items-center justify-center text-primary mb-2 border-2 border-primary/25">
          <Trophy size={32} />
        </div>
        <div className="space-y-2">
          <h2 className="text-xl font-bold text-foreground">{t("quizSolver.finishedTitle")}</h2>
          <p className="text-sm text-muted-foreground">{t("quizSolver.finishedDesc")}</p>
        </div>

        <div className="max-w-xs mx-auto bg-muted rounded-xl p-5 border border-border space-y-1">
          <div className="text-[10px] uppercase font-bold text-muted-foreground tracking-wider">{t("quizSolver.finishedScoreLabel")}</div>
          <div className="text-4xl font-bold text-primary">
            {finalScore.toFixed(1)} pkt
          </div>
          <div className="text-xs text-muted-foreground">
            {t("quizSolver.finishedMaxLabel", { max: totalPointsMax })}
          </div>
        </div>

        <div className="text-left space-y-3 pt-4 border-t border-border">
          <h4 className="text-[11px] uppercase font-bold text-muted-foreground tracking-wider mb-2">{t("quizSolver.finishedSummary")}</h4>
          {attemptDetails.questions.map((q, idx) => {
            const isCorrect = answersSummary[idx]?.isCorrect;
            return (
              <div key={q.id} className="flex items-start justify-between text-[13px] py-2 border-b border-border last:border-b-0">
                <span className="text-muted-foreground font-medium truncate max-w-[360px]">{idx + 1}. {q.content}</span>
                <div className="flex items-center gap-2 shrink-0 ml-4">
                  {getDifficultyBadge(q.difficulty)}
                  <span className={`font-semibold uppercase text-[10px] ${isCorrect ? "text-green-600 dark:text-green-400" : "text-red-500 dark:text-red-400"}`}>
                    {isCorrect ? t("quizSolver.finishedCorrectFeedback") : t("quizSolver.finishedIncorrectFeedback")}
                  </span>
                </div>
              </div>
            );
          })}
        </div>

        <div className="pt-6 flex gap-4">
          <SecondaryButton
            onClick={handleRetry}
            className="flex-1"
          >
            {t("quizSolver.finishedRetryBtn")}
          </SecondaryButton>
          <PrimaryButton
            onClick={handleCancel}
            className="flex-1"
          >
            {t("quizSolver.finishedBackBtn")}
          </PrimaryButton>
        </div>
      </div>
    );
  }

  const question: QuestionDto = attemptDetails.questions[currentQuestionIndex];
  const isMultiple = question.type === QuestionType.MultipleChoice;
  const currentQuestionSelected = selectedAnswers[question.id] || [];

  return (
    <div className="max-w-4xl mx-auto space-y-5 animate-in fade-in duration-300">
      {/* Top Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-bold text-foreground line-clamp-1">
            {attemptDetails.displayName}
          </h2>
        </div>
        <button
          onClick={() => {
            if (confirm(t("quizSolver.cancelConfirm"))) {
              handleCancel();
            }
          }}
          className="text-[12px] text-muted-foreground hover:text-destructive hover:underline font-semibold cursor-pointer transition-colors"
        >
          {t("quizSolver.cancelBtn")}
        </button>
      </div>

      <div className="flex flex-col md:flex-row gap-4 items-start">
        {/* Main Content Area: Question Card or Review Screen */}
        {isReviewMode ? (
          <div className="flex-1 w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden p-6 md:p-8 space-y-6">
            <h3 className="text-base font-bold text-foreground">
              {t("quizSolver.reviewTitle")}
            </h3>

            {/* Questions list with options and selected answer */}
            <div className="space-y-4">
              {attemptDetails.questions.map((q, idx) => {
                const selectedOptionIds = selectedAnswers[q.id] || [];
                const isMultiple = q.type === QuestionType.MultipleChoice;

                return (
                  <div
                    key={q.id}
                    className="p-4 rounded-xl bg-background/50 border border-border space-y-3"
                  >
                    <div className="space-y-1.5">
                      <div>
                        {getTypeBadge(q.type)}
                      </div>
                      <h4 className="text-[13px] font-semibold text-foreground leading-snug">
                        {idx + 1}. {q.content}
                      </h4>
                    </div>

                    <div className="space-y-1.5 pt-1">
                      {q.options.map((opt) => {
                        const isSelected = selectedOptionIds.includes(opt.id);
                        return (
                          <div
                            key={opt.id}
                            className={`p-2.5 rounded-lg text-xs border flex items-center gap-2.5 transition-colors ${
                              isSelected
                                ? "bg-primary/10 border-primary/40 text-foreground font-medium"
                                : "border-border/60 text-muted-foreground bg-card/40"
                            }`}
                          >
                            <div className={`w-3.5 h-3.5 ${isMultiple ? "rounded-xs" : "rounded-full"} border flex items-center justify-center shrink-0 ${
                              isSelected ? "bg-primary border-primary text-white" : "border-border"
                            }`}>
                              {isSelected && <Check size={9} strokeWidth={3} />}
                            </div>
                            <span className="leading-relaxed">{opt.content}</span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Bottom Actions for Review Screen */}
            <div className="pt-5 border-t border-border flex items-center justify-between gap-3">
              <SecondaryButton
                size="sm"
                onClick={() => {
                  setCurrentQuestionIndex(qCount - 1);
                  setIsReviewMode(false);
                }}
                icon={<ChevronLeft size={15} />}
              >
                {t("quizSolver.reviewBackToQuestions")}
              </SecondaryButton>

              <PrimaryButton
                size="sm"
                onClick={() => setShowSubmitModal(true)}
                disabled={!isAllAnswered}
                icon={<CheckCircle size={15} />}
                className="px-6 font-semibold"
              >
                {t("quizSolver.submitAttemptBtn")}
              </PrimaryButton>
            </div>
          </div>
        ) : (
          /* Main Question Card */
          <div className="flex-1 w-full bg-card rounded-xl border border-border shadow-sm overflow-hidden">
            <div className="bg-muted/60 px-6 py-3 border-b border-border flex justify-between items-center">
              <span className="text-xs font-medium text-muted-foreground">
                {t("quizSolver.questionProgress", { current: currentQuestionIndex + 1, total: qCount })}
              </span>
              <div className="flex items-center gap-2">
                {getTypeBadge(question.type)}
                {getDifficultyBadge(question.difficulty)}
                <span className="text-[10px] bg-primary/15 text-primary font-bold px-2 py-0.5 rounded uppercase">
                  {t("quizSolver.points", { points: question.points })}
                </span>
              </div>
            </div>

            <div className="h-1 bg-muted">
              <div
                className="h-full bg-primary transition-all duration-300"
                style={{ width: `${((currentQuestionIndex + 1) / qCount) * 100}%` }}
              />
            </div>

            <div className="p-6 md:p-8 space-y-6">
              <h3 className="text-base font-bold text-foreground leading-relaxed">
                {question.content}
              </h3>

              {/* Options */}
              <div className="space-y-2.5">
                {question.options.map((opt) => {
                  const isSelected = currentQuestionSelected.includes(opt.id);

                  let optionStyle = "border-border hover:bg-muted/40 hover:border-primary/30 text-foreground";
                  if (isSelected) {
                    optionStyle = "bg-primary/10 border-primary text-foreground font-medium";
                  }

                  return (
                    <button
                      key={opt.id}
                      onClick={() => handleToggleOption(question.id, opt.id, isMultiple)}
                      className={`w-full p-4 rounded-xl text-left text-[13px] border transition-all flex items-start gap-3.5 cursor-pointer ${optionStyle}`}
                    >
                      <div className="mt-0.5 flex-shrink-0">
                        <div className={`w-4.5 h-4.5 ${isMultiple ? "rounded-md" : "rounded-full"} border flex items-center justify-center transition-colors ${
                          isSelected 
                            ? "bg-primary border-primary text-white" 
                            : "border-border bg-card"
                        }`}>
                          {isSelected && <Check size={11} strokeWidth={3} />}
                        </div>
                      </div>
                      <span className="flex-1 leading-relaxed">{opt.content}</span>
                    </button>
                  );
                })}
              </div>

              {/* Bottom Navigation */}
              <div className="pt-5 border-t border-border flex items-center justify-between gap-3">
                <SecondaryButton
                  size="sm"
                  onClick={() => setCurrentQuestionIndex(prev => Math.max(0, prev - 1))}
                  disabled={currentQuestionIndex === 0}
                  icon={<ChevronLeft size={15} />}
                >
                  {t("quizSolver.prevBtn")}
                </SecondaryButton>

                {currentQuestionIndex < qCount - 1 ? (
                  <SecondaryButton
                    size="sm"
                    onClick={() => setCurrentQuestionIndex(prev => prev + 1)}
                    icon={<ChevronRight size={15} />}
                  >
                    {t("quizSolver.nextBtn")}
                  </SecondaryButton>
                ) : (
                  <PrimaryButton
                    size="sm"
                    onClick={() => setIsReviewMode(true)}
                    icon={<ChevronRight size={15} />}
                    className="px-5 font-semibold"
                  >
                    {t("quizSolver.goToReviewBtn")}
                  </PrimaryButton>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Question Selector Tiles on the right - 5 per row without card wrapper (sticky on scroll) */}
        <div className="grid grid-cols-5 gap-2 shrink-0 sticky top-6 self-start">
          {attemptDetails.questions.map((q, idx) => {
            const isCurrent = !isReviewMode && idx === currentQuestionIndex;
            const isAnswered = (selectedAnswers[q.id]?.length || 0) > 0;

            let pillStyle = "bg-card border-border text-muted-foreground hover:border-primary/40 hover:text-foreground";
            if (isCurrent) {
              pillStyle = "bg-primary text-white border-primary shadow-xs ring-2 ring-primary/30 font-bold";
            } else if (isAnswered) {
              pillStyle = "bg-primary/15 text-primary border-primary/40 font-semibold hover:bg-primary/25";
            }

            return (
              <button
                key={q.id}
                onClick={() => {
                  setCurrentQuestionIndex(idx);
                  setIsReviewMode(false);
                }}
                className={`w-9 h-9 rounded-lg border text-xs flex items-center justify-center transition-all cursor-pointer select-none ${pillStyle}`}
              >
                {idx + 1}
              </button>
            );
          })}
        </div>
      </div>

      <ConfirmModal
        isOpen={showSubmitModal}
        onClose={() => setShowSubmitModal(false)}
        onConfirm={handleConfirmSubmit}
        title={t("quizSolver.submitModalTitle")}
        message={t("quizSolver.submitModalMessage")}
        confirmBtnText={t("quizSolver.submitModalConfirm")}
        isDestructive={false}
      />
    </div>
  );
}
