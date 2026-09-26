import { useState, useEffect, useCallback, useRef } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { flashcardService } from "../services/flashcardService";
import type { FlashcardDeckDetailsDto, FlashcardDto } from "../types/flashcardTypes";
import { FlashcardStateEnum } from "../enums/flashcardEnums";
import { QuestionDifficulty, QuizAttemptStatus } from "../enums/quizEnums";
import { PrimaryButton, SecondaryButton, Badge, Alert, LoadingScreen } from "../components";
import { getScoreColorClass } from "../utils/scoreUtils";
import { ArrowLeft, ChevronLeft, FlipHorizontal } from "lucide-react";

export default function FlashcardStudyPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const [deck, setDeck] = useState<FlashcardDeckDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [isFinished, setIsFinished] = useState(false);
  const [isReviewOnly, setIsReviewOnly] = useState(false);
  const [cardStates, setCardStates] = useState<Record<string, FlashcardStateEnum>>({});
  const [sessionRatedCards, setSessionRatedCards] = useState<Record<string, FlashcardStateEnum>>({});

  const stateFilter = location.state as {
    fromPath?: string;
    attemptId?: string;
    easyCount?: number;
    mediumCount?: number;
    hardCount?: number;
  } | null;

  const attemptId = stateFilter?.attemptId;
  const hasCompletedAttempt = useRef(false);

  const fromPath = stateFilter?.fromPath || `/flashcards/${id}`;

  const [sessionCards, setSessionCards] = useState<FlashcardDto[]>([]);

  const fetchDeckDetails = async () => {
    if (!id) return;
    setLoading(true);
    setErrorMsg(null);
    try {
      const data = await flashcardService.getFlashcardDeckDetails(id);
      setDeck(data);

      const isEasy = (diff?: QuestionDifficulty | string | number) =>
        diff === QuestionDifficulty.Easy || diff === 1 || diff === "Easy" || diff === "1";
      const isHard = (diff?: QuestionDifficulty | string | number) =>
        diff === QuestionDifficulty.Hard || diff === 3 || diff === "Hard" || diff === "3";

      let filteredCards = [...data.cards];
      const hasDifficultyFilterInState =
        stateFilter?.easyCount !== undefined ||
        stateFilter?.mediumCount !== undefined ||
        stateFilter?.hardCount !== undefined;

      const hasDifficultyFilterInDeck =
        data.easyCardCountPerAttempt != null ||
        data.mediumCardCountPerAttempt != null ||
        data.hardCardCountPerAttempt != null;

      if (hasDifficultyFilterInState || hasDifficultyFilterInDeck) {
        const easyLimit = stateFilter?.easyCount ?? data.easyCardCountPerAttempt ?? 999;
        const mediumLimit = stateFilter?.mediumCount ?? data.mediumCardCountPerAttempt ?? 999;
        const hardLimit = stateFilter?.hardCount ?? data.hardCardCountPerAttempt ?? 999;

        const easyCards = data.cards.filter((c) => isEasy(c.difficulty)).slice(0, easyLimit);
        const hardCards = data.cards.filter((c) => isHard(c.difficulty)).slice(0, hardLimit);
        const mediumCards = data.cards.filter((c) => !isEasy(c.difficulty) && !isHard(c.difficulty)).slice(0, mediumLimit);

        filteredCards = [...easyCards, ...mediumCards, ...hardCards];
      } else {
        const targetCount = data.cardCountPerAttempt && data.cardCountPerAttempt > 0
          ? data.cardCountPerAttempt
          : data.cards.length;
        filteredCards = data.cards.slice(0, targetCount);
      }
      setSessionCards(filteredCards);

      const initialMap: Record<string, FlashcardStateEnum> = {};
      data.cards.forEach((c) => {
        initialMap[c.id] = c.state;
      });
      setCardStates(initialMap);

      let savedAttemptMap: Record<string, FlashcardStateEnum> | null = null;

      if (attemptId) {
        const foundAttempt = data.attempts.find((a) => a.id === attemptId);
        if (foundAttempt) {
          if (foundAttempt.cardStates && Object.keys(foundAttempt.cardStates).length > 0) {
            const savedMap: Record<string, FlashcardStateEnum> = {};
            Object.entries(foundAttempt.cardStates).forEach(([cId, st]) => {
              savedMap[cId] = Number(st) as FlashcardStateEnum;
            });
            setCardStates(savedMap);
            setSessionRatedCards(savedMap);
            savedAttemptMap = savedMap;
          }
          if (foundAttempt.status === QuizAttemptStatus.Completed) {
            setIsReviewOnly(true);
            setIsFinished(true);
          }
        }
      }

      if (savedAttemptMap) {
        const firstUnratedIndex = filteredCards.findIndex((c) => savedAttemptMap![c.id] === undefined);
        if (firstUnratedIndex !== -1) {
          setCurrentIndex(firstUnratedIndex);
        } else if (filteredCards.length > 0) {
          setCurrentIndex(filteredCards.length - 1);
        }
      }
    } catch (err: any) {
      setErrorMsg(err.message || "Błąd podczas wczytywania fiszek.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeckDetails();
  }, [id]);

  const handleGoBack = () => {
    navigate(fromPath);
  };

  const currentCard: FlashcardDto | undefined = sessionCards[currentIndex];

  const handleSetCardState = async (state: FlashcardStateEnum) => {
    if (!currentCard || isReviewOnly) return;

    setSessionRatedCards((prev) => ({ ...prev, [currentCard.id]: state }));
    setCardStates((prev) => ({ ...prev, [currentCard.id]: state }));

    try {
      if (attemptId) {
        await flashcardService.submitFlashcardAttemptCardState(attemptId, currentCard.id, state);
      } else {
        await flashcardService.updateFlashcardState(currentCard.id, state);
      }
    } catch (err) {
      console.warn("[FlashcardStudyPage] Failed to update card state:", err);
    }

    if (currentIndex < sessionCards.length - 1) {
      setIsFlipped(false);
      setCurrentIndex((prev) => prev + 1);
    } else {
      setIsFinished(true);
    }
  };

  const handleConfirmCompleteAttempt = async () => {
    if (attemptId && !hasCompletedAttempt.current) {
      hasCompletedAttempt.current = true;
      try {
        await flashcardService.completeFlashcardAttempt(attemptId);
      } catch (err) {
        console.warn("[FlashcardStudyPage] Failed to complete attempt:", err);
      }
    }
    navigate(fromPath);
  };

  const handlePrev = () => {
    if (currentIndex > 0) {
      setIsFlipped(false);
      setCurrentIndex((prev) => prev - 1);
    }
  };

  const handleNext = () => {
    if (currentIndex < sessionCards.length - 1) {
      setIsFlipped(false);
      setCurrentIndex((prev) => prev + 1);
    } else {
      setIsFinished(true);
    }
  };

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (isFinished || isReviewOnly || !currentCard) return;

      if (e.code === "Space") {
        e.preventDefault();
        setIsFlipped((prev) => !prev);
      } else if (e.code === "ArrowLeft") {
        e.preventDefault();
        handlePrev();
      } else if (e.code === "ArrowRight") {
        e.preventDefault();
        handleNext();
      } else if (e.code === "Digit1" || e.code === "Numpad1") {
        e.preventDefault();
        handleSetCardState(FlashcardStateEnum.Learning);
      } else if (e.code === "Digit2" || e.code === "Numpad2") {
        e.preventDefault();
        handleSetCardState(FlashcardStateEnum.Mastered);
      }
    },
    [isFinished, isReviewOnly, currentCard, currentIndex, sessionCards]
  );

  useEffect(() => {
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [handleKeyDown]);

  if (loading) {
    return <LoadingScreen message={t("flashcards.loadingStudy")} />;
  }

  if (errorMsg || !deck || sessionCards.length === 0) {
    return (
      <div className="flex-1 p-6 w-full space-y-4">
        <button
          onClick={() => navigate(fromPath)}
          className="flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
        >
          <ArrowLeft size={14} /> {t("flashcardDetails.backBtn")}
        </button>
        <Alert variant="error" message={errorMsg || t("flashcards.noCardsInSession")} />
      </div>
    );
  }

  if (isFinished || isReviewOnly) {
    const isCardEasy = (diff?: QuestionDifficulty | string | number) =>
      diff === QuestionDifficulty.Easy || (diff as any) === 1 || (diff as any) === "Easy" || (diff as any) === "1";
    const isCardHard = (diff?: QuestionDifficulty | string | number) =>
      diff === QuestionDifficulty.Hard || (diff as any) === 3 || (diff as any) === "Hard" || (diff as any) === "3";

    const getCardPoints = (diff?: QuestionDifficulty | string | number) => {
      if (isCardEasy(diff)) return 1;
      if (isCardHard(diff)) return 3;
      return 2;
    };

    const totalCards = sessionCards.length;
    const masteredCount = Object.values(cardStates).filter((s) => s === FlashcardStateEnum.Mastered).length;
    const maxAttemptPoints = sessionCards.reduce((sum, card) => sum + getCardPoints(card.difficulty), 0);
    const earnedAttemptPoints = sessionCards.reduce((sum, card) => {
      const state = cardStates[card.id] || card.state;
      return state === FlashcardStateEnum.Mastered ? sum + getCardPoints(card.difficulty) : sum;
    }, 0);

    const scorePct = maxAttemptPoints > 0 ? Math.round((earnedAttemptPoints / maxAttemptPoints) * 100) : 0;

    return (
      <div className="w-full space-y-5 animate-in fade-in duration-300">
        {/* Top Header */}
        <div className="flex items-center gap-3.5 min-w-0">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("flashcardDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0">
            {deck.courseName && (
              <Badge variant="secondary" className="self-start mb-1">
                {deck.courseName}
              </Badge>
            )}
            <h1 className="text-xl font-bold text-foreground break-words leading-tight">
              {deck.name}
            </h1>
          </div>
        </div>

        <div className="flex flex-col md:flex-row gap-4 items-start">
          {/* Main Report Card */}
          <div className="flex-1 w-full order-2 md:order-1 bg-card rounded-xl border border-border shadow-sm overflow-hidden p-6 md:p-8 space-y-6">
            <div className="flex items-center justify-between gap-3 pb-4 border-b border-border">
              <div className="flex items-center gap-3">
                <span className="text-base font-bold text-foreground">
                  {masteredCount} / {totalCards} {t("flashcardDetails.cardsUnit")}
                </span>
                <span className={`text-base font-bold tabular-nums ${getScoreColorClass(scorePct)}`}>
                  {scorePct}%
                </span>
              </div>
            </div>

            {/* Read-Only Flashcards List */}
            <div className="space-y-4">
              <div className="space-y-3.5 w-full">
                {sessionCards.map((card, idx) => {
                  const state = cardStates[card.id] || card.state;
                  const isMastered = state === FlashcardStateEnum.Mastered;
                  const isCardEasy = (diff?: QuestionDifficulty | string | number) =>
                    diff === QuestionDifficulty.Easy || (diff as any) === 1 || (diff as any) === "Easy" || (diff as any) === "1";
                  const isCardHard = (diff?: QuestionDifficulty | string | number) =>
                    diff === QuestionDifficulty.Hard || (diff as any) === 3 || (diff as any) === "Hard" || (diff as any) === "3";

                  const easyCard = isCardEasy(card.difficulty);
                  const hardCard = isCardHard(card.difficulty);
                  const diffLabel = easyCard ? t("quizSolver.difficulty.easy") : hardCard ? t("quizSolver.difficulty.hard") : t("quizSolver.difficulty.medium");
                  const diffTextColor = easyCard ? "text-emerald-400" : hardCard ? "text-rose-400" : "text-amber-400";
                  const stateTextColor = isMastered ? "text-emerald-400" : "text-amber-400";

                  return (
                    <div
                      key={card.id}
                      id={`report-card-${idx}`}
                      className="p-4 md:p-5 rounded-xl bg-card border border-border space-y-3"
                    >
                      <div className="flex items-center justify-between gap-2 border-b border-border/50 pb-2.5">
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-bold text-foreground bg-secondary px-2.5 py-0.5 rounded-md border border-border">
                            #{idx + 1}
                          </span>
                          <span className={`text-[11px] font-semibold uppercase tracking-wider ${diffTextColor}`}>
                            {diffLabel}
                          </span>
                        </div>
                        <span className={`text-xs font-bold ${stateTextColor}`}>
                          {isMastered ? t("flashcardDetails.stats.mastered") : t("flashcardDetails.stats.learning")}
                        </span>
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3 pt-1">
                        <div className="p-3.5 rounded-xl bg-background border border-border/80 flex items-center">
                          <p className="text-xs md:text-sm font-bold text-foreground leading-snug">
                            {card.front}
                          </p>
                        </div>

                        <div className="p-3.5 rounded-xl bg-secondary/40 border border-border/80 flex items-center">
                          <p className="text-xs md:text-sm font-medium text-foreground/90 leading-relaxed">
                            {card.back}
                          </p>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Bottom Actions for Review Screen - matching QuizSolverPage 1:1 */}
            {!isReviewOnly && (
              <div className="pt-5 border-t border-border flex items-center justify-between gap-3">
                <SecondaryButton
                  size="sm"
                  onClick={() => {
                    setCurrentIndex(sessionCards.length - 1);
                    setIsFinished(false);
                  }}
                >
                  {t("flashcards.reviewBackToCards")}
                </SecondaryButton>

                <PrimaryButton
                  size="sm"
                  onClick={handleConfirmCompleteAttempt}
                  className="px-6 font-semibold"
                >
                  {t("flashcards.submitAttemptBtn")}
                </PrimaryButton>
              </div>
            )}
          </div>

          {/* Right sticky card navigator tiles - matching QuizSolverPage 1:1 */}
          <div className="flex flex-wrap items-center gap-2 md:grid md:grid-cols-5 w-full md:w-auto shrink-0 md:sticky md:top-6 self-start order-1 md:order-2">
            {sessionCards.map((card, idx) => {
              const state = cardStates[card.id] || card.state;
              const isMastered = state === FlashcardStateEnum.Mastered;
              const ringBorder = isMastered
                ? "border-emerald-500/70 dark:border-emerald-500/60"
                : "border-amber-500/70 dark:border-amber-500/60";

              return (
                <button
                  key={card.id}
                  type="button"
                  onClick={() => {
                    const el = document.getElementById(`report-card-${idx}`);
                    if (el) {
                      el.scrollIntoView({ behavior: "smooth", block: "center" });
                    }
                  }}
                  className={`w-9 h-9 md:w-10 md:h-10 rounded-xl bg-card border ${ringBorder} text-foreground font-bold text-xs md:text-sm flex items-center justify-center transition-all duration-200 ease-out hover:scale-105 active:scale-95 cursor-pointer select-none shadow-2xs`}
                  title={`#${idx + 1} - ${isMastered ? t("flashcardDetails.stats.mastered") : t("flashcardDetails.stats.learning")}`}
                >
                  {idx + 1}
                </button>
              );
            })}
          </div>
        </div>
      </div>
    );
  }

  if (!currentCard) return null;

  const sessionState = sessionRatedCards[currentCard.id];
  const isLearningSelected = sessionState === FlashcardStateEnum.Learning;
  const isMasteredSelected = sessionState === FlashcardStateEnum.Mastered;

  const isCardEasy = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Easy || (diff as any) === 1 || (diff as any) === "Easy" || (diff as any) === "1";
  const isCardHard = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Hard || (diff as any) === 3 || (diff as any) === "Hard" || (diff as any) === "3";

  const easyCard = isCardEasy(currentCard.difficulty);
  const hardCard = isCardHard(currentCard.difficulty);
  const diffLabel = easyCard ? t("quizSolver.difficulty.easy") : hardCard ? t("quizSolver.difficulty.hard") : t("quizSolver.difficulty.medium");
  const diffTextColor = easyCard ? "text-emerald-400" : hardCard ? "text-rose-400" : "text-amber-400";

  return (
    <div className="w-full space-y-5 animate-in fade-in duration-300">
      {/* Top Header */}
      <div>
        <SecondaryButton
          type="button"
          onClick={handleGoBack}
          aria-label={t("flashcards.exitStudy")}
          icon={<ChevronLeft size={22} strokeWidth={2.25} />}
          className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
        />
      </div>

      <div className="flex flex-col md:flex-row gap-4 items-start">
        {/* Main Card Column */}
        <div className="flex-1 w-full space-y-4 order-2 md:order-1">
          {/* Progress Bar - matching exact question card width */}
          <div className="w-full h-2 bg-secondary rounded-full overflow-hidden border border-border/60">
            <div
              className="h-full bg-primary transition-all duration-300 ease-out"
              style={{ width: `${((currentIndex + 1) / sessionCards.length) * 100}%` }}
            />
          </div>

          {/* 3D Flip Card Container */}
          <div
            onClick={() => setIsFlipped((prev) => !prev)}
            className="relative z-20 w-full h-[320px] md:h-[380px] cursor-pointer perspective-1000 select-none group"
          >
            <div
              className={`relative w-full h-full duration-500 transform-style-3d transition-transform ${
                isFlipped ? "rotate-y-180" : ""
              }`}
            >
              {/* FRONT side (Term / Key) */}
              <div className="absolute inset-0 w-full h-full rounded-2xl bg-card border-2 border-border p-6 md:p-8 flex flex-col justify-between items-center text-center backface-hidden shadow-sm group-hover:border-primary/50 transition-colors">
                <div className="flex items-center justify-between w-full">
                  <span className={`text-[11px] font-extrabold uppercase tracking-wider ${diffTextColor}`}>
                    {diffLabel}
                  </span>
                  <span className="text-xs font-medium text-muted-foreground flex items-center gap-1">
                    <FlipHorizontal size={13} /> {t("flashcards.flipHint")}
                  </span>
                </div>

                <div className="my-auto px-4">
                  <h2 className="text-xl md:text-2xl font-extrabold text-foreground leading-snug">
                    {currentCard.front}
                  </h2>
                </div>

                <div className="text-[11px] font-semibold text-muted-foreground">
                  {currentIndex + 1} / {deck.cards.length}
                </div>
              </div>

              {/* BACK side (Definition / Value) */}
              <div className="absolute inset-0 w-full h-full rounded-2xl bg-card border-2 border-primary/40 p-6 md:p-8 flex flex-col justify-between items-center text-center backface-hidden rotate-y-180 shadow-sm">
                <div className="flex items-center justify-between w-full">
                  <span className={`text-[11px] font-extrabold uppercase tracking-wider ${diffTextColor}`}>
                    {diffLabel}
                  </span>
                  <span className="text-xs font-medium text-muted-foreground flex items-center gap-1">
                    <FlipHorizontal size={13} /> {t("flashcards.flipBackHint")}
                  </span>
                </div>

                <div className="my-auto px-4 overflow-y-auto max-h-[220px]">
                  <p className="text-sm md:text-base font-semibold text-foreground/90 leading-relaxed">
                    {currentCard.back}
                  </p>
                </div>

                <div className="text-[11px] font-semibold text-muted-foreground">
                  {currentIndex + 1} / {deck.cards.length}
                </div>
              </div>
            </div>
          </div>

          {/* Action Controls & Navigation */}
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3.5 w-full">
              <button
                type="button"
                onClick={() => handleSetCardState(FlashcardStateEnum.Learning)}
                className={`w-full py-3.5 px-4 rounded-xl text-sm font-extrabold uppercase tracking-wider transition-all duration-200 cursor-pointer bg-card border ${
                  isLearningSelected
                    ? "border-amber-500 text-amber-400 opacity-100 shadow-xs"
                    : isMasteredSelected
                      ? "border-border text-amber-400/50 opacity-40 hover:opacity-70"
                      : "border-border text-amber-400 hover:bg-muted/60 opacity-100"
                }`}
              >
                {t("flashcards.learningBtn")}
              </button>

              <button
                type="button"
                onClick={() => handleSetCardState(FlashcardStateEnum.Mastered)}
                className={`w-full py-3.5 px-4 rounded-xl text-sm font-extrabold uppercase tracking-wider transition-all duration-200 cursor-pointer bg-card border ${
                  isMasteredSelected
                    ? "border-emerald-500 text-emerald-400 opacity-100 shadow-xs"
                    : isLearningSelected
                      ? "border-border text-emerald-400/50 opacity-40 hover:opacity-70"
                      : "border-border text-emerald-400 hover:bg-muted/60 opacity-100"
                }`}
              >
                {t("flashcards.masteredBtn")}
              </button>
            </div>

            {/* Prev / Next controls - matching QuizSolverPage 1:1 */}
            <div className="flex items-center justify-between gap-3 pt-2">
              <SecondaryButton
                size="sm"
                onClick={handlePrev}
                disabled={currentIndex === 0}
              >
                {t("quizSolver.prevBtn")}
              </SecondaryButton>

              {currentIndex < sessionCards.length - 1 ? (
                <PrimaryButton
                  size="sm"
                  onClick={handleNext}
                  className="px-5 font-semibold"
                >
                  {t("quizSolver.nextBtn")}
                </PrimaryButton>
              ) : (
                <PrimaryButton
                  size="sm"
                  onClick={() => setIsFinished(true)}
                  className="px-5 font-semibold"
                >
                  {t("quizSolver.goToReviewBtn")}
                </PrimaryButton>
              )}
            </div>
          </div>
        </div>

        {/* Right Sticky Card Navigator Tiles - matching QuizSolverPage 1:1 */}
        <div className="w-full md:w-auto shrink-0 md:sticky md:top-6 self-start order-1 md:order-2">
          <div className="flex flex-wrap items-center gap-2 md:grid md:grid-cols-5">
            {sessionCards.map((c, idx) => {
              const isCurrent = idx === currentIndex;
              const ratedState = sessionRatedCards[c.id];
              const isMastered = ratedState === FlashcardStateEnum.Mastered;
              const isLearning = ratedState === FlashcardStateEnum.Learning;

              let ringBorder = "border-border text-muted-foreground/60 font-medium hover:border-border/80 hover:text-foreground";
              if (isCurrent) {
                ringBorder = "border-2 border-primary text-primary font-bold shadow-xs";
              } else if (isMastered) {
                ringBorder = "border border-emerald-500/70 text-emerald-400 font-bold hover:border-emerald-500";
              } else if (isLearning) {
                ringBorder = "border border-amber-500/70 text-amber-400 font-bold hover:border-amber-500";
              }

              return (
                <button
                  key={c.id}
                  type="button"
                  onClick={() => {
                    setCurrentIndex(idx);
                    setIsFlipped(false);
                  }}
                  className={`w-9 h-9 md:w-10 md:h-10 rounded-xl bg-card ${ringBorder} text-xs md:text-sm flex items-center justify-center transition-all duration-200 ease-out hover:scale-105 active:scale-95 cursor-pointer select-none shadow-2xs`}
                >
                  {idx + 1}
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
