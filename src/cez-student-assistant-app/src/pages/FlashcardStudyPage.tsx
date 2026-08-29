import { useState, useEffect, useCallback, useRef } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { flashcardService } from "../services/flashcardService";
import type { FlashcardDeckDetailsDto, FlashcardDto } from "../types/flashcardTypes";
import { FlashcardStateEnum } from "../enums/flashcardEnums";
import { QuestionDifficulty } from "../enums/quizEnums";
import Card from "../components/Card";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import { Alert } from "../components/Alert";
import LoadingScreen from "../components/LoadingScreen";
import { getScoreColorClass } from "../utils/scoreUtils";
import { ArrowLeft, RotateCcw, CheckCircle2, BookOpen, ChevronLeft, ChevronRight, FlipHorizontal, Trophy } from "lucide-react";

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
  const [cardStates, setCardStates] = useState<Record<string, FlashcardStateEnum>>({});

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
      if (
        stateFilter?.easyCount !== undefined ||
        stateFilter?.mediumCount !== undefined ||
        stateFilter?.hardCount !== undefined
      ) {
        const easyLimit = stateFilter.easyCount ?? 999;
        const mediumLimit = stateFilter.mediumCount ?? 999;
        const hardLimit = stateFilter.hardCount ?? 999;

        const easyCards = data.cards.filter((c) => isEasy(c.difficulty)).slice(0, easyLimit);
        const hardCards = data.cards.filter((c) => isHard(c.difficulty)).slice(0, hardLimit);
        const mediumCards = data.cards.filter((c) => !isEasy(c.difficulty) && !isHard(c.difficulty)).slice(0, mediumLimit);

        filteredCards = [...easyCards, ...mediumCards, ...hardCards];
      }
      setSessionCards(filteredCards);

      const initialMap: Record<string, FlashcardStateEnum> = {};
      data.cards.forEach((c) => {
        initialMap[c.id] = c.state;
      });
      setCardStates(initialMap);
    } catch (err: any) {
      setErrorMsg(err.message || "Błąd podczas wczytywania fiszek.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeckDetails();
  }, [id]);

  const currentCard: FlashcardDto | undefined = sessionCards[currentIndex];

  const handleSetCardState = async (state: FlashcardStateEnum) => {
    if (!currentCard) return;

    setCardStates((prev) => ({ ...prev, [currentCard.id]: state }));

    try {
      await flashcardService.updateFlashcardState(currentCard.id, state);
    } catch (err) {
      console.warn("[FlashcardStudyPage] Failed to update card state:", err);
    }

    // Move to next card or finish
    if (currentIndex < sessionCards.length - 1) {
      setIsFlipped(false);
      setCurrentIndex((prev) => prev + 1);
    } else {
      setIsFinished(true);
      if (attemptId && !hasCompletedAttempt.current) {
        hasCompletedAttempt.current = true;
        const updatedStates = { ...cardStates, [currentCard.id]: state };
        const masteredCount = Object.values(updatedStates).filter((s) => s === FlashcardStateEnum.Mastered).length;
        const learningCount = Object.values(updatedStates).filter((s) => s === FlashcardStateEnum.Learning).length;
        flashcardService.completeFlashcardAttempt(attemptId, masteredCount, learningCount).catch((err) => {
          console.warn("[FlashcardStudyPage] Failed to complete attempt:", err);
        });
      }
    }
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

  // Keyboard shortcut handlers
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (isFinished || !currentCard) return;

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
    [isFinished, currentCard, currentIndex, sessionCards]
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
      <div className="flex-1 p-6 max-w-3xl mx-auto space-y-4">
        <button
          onClick={() => navigate(fromPath)}
          className="flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
        >
          <ArrowLeft size={14} /> Powrót
        </button>
        <Alert variant="error" message={errorMsg || "Brak fiszek w tej sesji nauki."} />
      </div>
    );
  }

  // Final Summary Screen
  if (isFinished) {
    const totalCards = sessionCards.length;
    const masteredCount = Object.values(cardStates).filter((s) => s === FlashcardStateEnum.Mastered).length;
    const learningCount = Object.values(cardStates).filter((s) => s === FlashcardStateEnum.Learning).length;
    const scorePct = Math.round(((masteredCount * 1.0 + learningCount * 0.5) / totalCards) * 100);

    return (
      <div className="flex-1 p-6 md:p-8 max-w-2xl mx-auto w-full flex flex-col items-center justify-center space-y-6 text-center animate-in fade-in duration-300">
        <Card borderLeftPrimary className="p-8 space-y-6 w-full flex flex-col items-center">
          <div className="w-16 h-16 rounded-full bg-primary/10 text-primary border border-primary/20 flex items-center justify-center">
            <Trophy size={32} />
          </div>

          <div className="space-y-1">
            <h2 className="text-2xl font-black text-foreground">Koniec nauki w zestawie!</h2>
            <p className="text-xs text-muted-foreground font-medium">{deck.name}</p>
          </div>

          <div className="flex flex-col items-center gap-1">
            <span className={`text-4xl font-black tabular-nums ${getScoreColorClass(scorePct)}`}>
              {scorePct}%
            </span>
            <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
              Osiągnięty progres
            </span>
          </div>

          <div className="grid grid-cols-2 gap-4 w-full pt-4 border-t border-border/60">
            <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/20 text-center">
              <div className="text-xl font-black text-emerald-400 tabular-nums">{masteredCount}</div>
              <div className="text-xs font-bold text-emerald-300/80">Opanowane (Umiem)</div>
            </div>
            <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/20 text-center">
              <div className="text-xl font-black text-amber-400 tabular-nums">{learningCount}</div>
              <div className="text-xs font-bold text-amber-300/80">Do powtórki (Uczę się)</div>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-3 w-full pt-2">
            <SecondaryButton
              fullWidth
              onClick={() => {
                setIsFinished(false);
                setCurrentIndex(0);
                setIsFlipped(false);
              }}
              icon={<RotateCcw size={15} />}
            >
              Powtórz od nowa
            </SecondaryButton>
            <PrimaryButton
              fullWidth
              onClick={() => navigate(fromPath)}
            >
              Wróć do zestawu
            </PrimaryButton>
          </div>
        </Card>
      </div>
    );
  }

  if (!currentCard) return null;

  const cardState = cardStates[currentCard.id] || currentCard.state;

  return (
    <div className="flex-1 p-4 md:p-8 max-w-3xl mx-auto w-full flex flex-col justify-between space-y-6 animate-in fade-in duration-300">
      {/* Top Header & Progress */}
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-4">
          <button
            onClick={() => navigate(fromPath)}
            className="flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
          >
            <ArrowLeft size={14} /> {t("flashcards.exitStudy")}
          </button>

          <div className="text-xs font-bold text-foreground bg-secondary px-3 py-1 rounded-full border border-border">
            {t("flashcards.cardProgress", { current: currentIndex + 1, total: sessionCards.length })}
          </div>
        </div>

        {/* Progress Bar */}
        <div className="w-full h-2 bg-secondary rounded-full overflow-hidden border border-border/60">
          <div
            className="h-full bg-primary transition-all duration-300 ease-out"
            style={{ width: `${((currentIndex + 1) / sessionCards.length) * 100}%` }}
          />
        </div>
      </div>

      {/* 3D Flip Card Container */}
      <div
        onClick={() => setIsFlipped((prev) => !prev)}
        className="w-full h-[320px] md:h-[380px] cursor-pointer perspective-1000 select-none group"
      >
        <div
          className={`relative w-full h-full duration-500 transform-style-3d transition-transform ${
            isFlipped ? "rotate-y-180" : ""
          }`}
        >
          {/* FRONT side (Term / Key) */}
          <div className="absolute inset-0 w-full h-full rounded-2xl bg-card border-2 border-border p-6 md:p-8 flex flex-col justify-between items-center text-center backface-hidden shadow-2xl group-hover:border-primary/50 transition-colors">
            <div className="flex items-center justify-between w-full">
              <span className="text-[11px] font-bold text-primary uppercase tracking-wider bg-primary/10 px-2.5 py-1 rounded-md border border-primary/20">
                {t("flashcards.frontLabel")}
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
          <div className="absolute inset-0 w-full h-full rounded-2xl bg-card border-2 border-primary/40 p-6 md:p-8 flex flex-col justify-between items-center text-center backface-hidden rotate-y-180 shadow-2xl">
            <div className="flex items-center justify-between w-full">
              <span className="text-[11px] font-bold text-emerald-400 uppercase tracking-wider bg-emerald-500/10 px-2.5 py-1 rounded-md border border-emerald-500/20">
                {t("flashcards.backLabel")}
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
        <div className="grid grid-cols-2 gap-3 max-w-md mx-auto">
          <SecondaryButton
            size="md"
            onClick={() => handleSetCardState(FlashcardStateEnum.Learning)}
            className={`!border-amber-500/30 hover:!bg-amber-500/20 ${
              cardState === FlashcardStateEnum.Learning ? "bg-amber-500/20 border-amber-500 text-amber-400 font-bold" : ""
            }`}
            icon={<BookOpen size={16} className="text-amber-400" />}
          >
            {t("flashcards.learningBtn")}
          </SecondaryButton>

          <PrimaryButton
            size="md"
            onClick={() => handleSetCardState(FlashcardStateEnum.Mastered)}
            className={`!bg-emerald-600 hover:!bg-emerald-500 ${
              cardState === FlashcardStateEnum.Mastered ? "ring-2 ring-emerald-400 font-bold" : ""
            }`}
            icon={<CheckCircle2 size={16} />}
          >
            {t("flashcards.masteredBtn")}
          </PrimaryButton>
        </div>

        {/* Prev / Next controls */}
        <div className="flex items-center justify-between text-xs font-semibold text-muted-foreground pt-2">
          <button
            onClick={handlePrev}
            disabled={currentIndex === 0}
            className="flex items-center gap-1 hover:text-foreground disabled:opacity-30 cursor-pointer transition-colors"
          >
            <ChevronLeft size={16} /> {t("flashcards.prevCard")}
          </button>

          <span className="text-[11px] text-muted-foreground/80 hidden sm:inline">
            {t("flashcards.shortcutsHint")}
          </span>

          <button
            onClick={handleNext}
            className="flex items-center gap-1 hover:text-foreground cursor-pointer transition-colors"
          >
            {t("flashcards.nextCard")} <ChevronRight size={16} />
          </button>
        </div>
      </div>
    </div>
  );
}
