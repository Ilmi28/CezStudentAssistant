import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import Modal from "./Modal";
import Input from "./Input";
import { PrimaryButton, SecondaryButton } from "./Button";
import { Alert } from "./Alert";
import MultiSegmentProgressBar from "./MultiSegmentProgressBar";
import { flashcardService } from "../services/flashcardService";

interface EditFlashcardDeckModalProps {
  isOpen: boolean;
  onClose: () => void;
  deckId: string;
  initialName: string;
  initialEasyCount?: number | null;
  initialMediumCount?: number | null;
  initialHardCount?: number | null;
  easyInPool: number;
  mediumInPool: number;
  hardInPool: number;
  onSuccess: () => void;
}

const formatCardCount = (n: number, lang: string) => {
  if (lang.startsWith("en")) {
    return n === 1 ? "1 flashcard" : `${n} flashcards`;
  }
  if (n === 1) return "1 fiszka";
  if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) return `${n} fiszki`;
  return `${n} fiszek`;
};

export function EditFlashcardDeckModal({
  isOpen,
  onClose,
  deckId,
  initialName,
  initialEasyCount,
  initialMediumCount,
  initialHardCount,
  easyInPool,
  mediumInPool,
  hardInPool,
  onSuccess,
}: EditFlashcardDeckModalProps) {
  const { t, i18n } = useTranslation();
  const [name, setName] = useState(initialName);
  const [easyCount, setEasyCount] = useState<number>(
    initialEasyCount ?? Math.min(2, easyInPool)
  );
  const [mediumCount, setMediumCount] = useState<number>(
    initialMediumCount ?? Math.min(2, mediumInPool)
  );
  const [hardCount, setHardCount] = useState<number>(
    initialHardCount ?? Math.min(1, hardInPool)
  );

  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setName(initialName);
      setEasyCount(initialEasyCount ?? Math.min(2, easyInPool));
      setMediumCount(initialMediumCount ?? Math.min(2, mediumInPool));
      setHardCount(initialHardCount ?? Math.min(1, hardInPool));
      setErrorMsg(null);
    }
  }, [
    isOpen,
    initialName,
    initialEasyCount,
    initialMediumCount,
    initialHardCount,
    easyInPool,
    mediumInPool,
    hardInPool,
  ]);

  const totalSelected = easyCount + mediumCount + hardCount;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setErrorMsg(t("auth.emptyFields"));
      return;
    }

    if (totalSelected <= 0) {
      setErrorMsg(t("flashcardDetails.atLeastOneCardError"));
      return;
    }

    setIsSubmitting(true);
    setErrorMsg(null);

    try {
      await flashcardService.updateFlashcardDeck(
        deckId,
        name.trim(),
        totalSelected,
        easyCount,
        mediumCount,
        hardCount
      );
      onSuccess();
      onClose();
    } catch (err: any) {
      setErrorMsg(err.message || t("common.genericError"));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setErrorMsg(null);
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("flashcardDetails.editModalTitle")}
      maxWidth="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {errorMsg && <Alert variant="error" message={errorMsg} />}

        <Input
          label={t("quizDetails.quizNameLabel")}
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            if (errorMsg) setErrorMsg(null);
          }}
          required
        />

        {/* Difficulty Distribution Section */}
        <div className="space-y-3.5 pt-3 border-t border-border/60">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-foreground font-medium">
              {t("flashcardDetails.editModalDesc")}
            </span>
            <span className="text-foreground font-bold text-xs bg-secondary px-2.5 py-0.5 rounded-full border border-border">
              {formatCardCount(totalSelected, i18n.language)}
            </span>
          </div>

          {/* Segmented Live Preview Bar */}
          <MultiSegmentProgressBar
            segments={[
              { id: "easy", value: easyCount, colorClass: "bg-emerald-500", customTooltip: `${t("quizSolver.difficulty.easy")} (${easyCount})` },
              { id: "medium", value: mediumCount, colorClass: "bg-amber-500", customTooltip: `${t("quizSolver.difficulty.medium")} (${mediumCount})` },
              { id: "hard", value: hardCount, colorClass: "bg-rose-500", customTooltip: `${t("quizSolver.difficulty.hard")} (${hardCount})` },
            ]}
            heightClass="h-3.5"
          />

          {/* 3 Spacious Control Rows */}
          <div className="space-y-2">
            {/* Easy Row */}
            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">{t("quizSolver.difficulty.easy")}</span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {easyCount} / {easyInPool} {t("quizDetails.inBank")}
                </span>
                <div className="flex items-center gap-1.5">
                  <button
                    type="button"
                    onClick={() => setEasyCount((prev) => Math.max(0, prev - 1))}
                    disabled={easyCount <= 0}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    -
                  </button>
                  <span className="w-6 text-center text-xs font-bold tabular-nums text-foreground">
                    {easyCount}
                  </span>
                  <button
                    type="button"
                    onClick={() => setEasyCount((prev) => Math.min(easyInPool, prev + 1))}
                    disabled={easyCount >= easyInPool}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>

            {/* Medium Row */}
            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-amber-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">{t("quizSolver.difficulty.medium")}</span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {mediumCount} / {mediumInPool} {t("quizDetails.inBank")}
                </span>
                <div className="flex items-center gap-1.5">
                  <button
                    type="button"
                    onClick={() => setMediumCount((prev) => Math.max(0, prev - 1))}
                    disabled={mediumCount <= 0}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    -
                  </button>
                  <span className="w-6 text-center text-xs font-bold tabular-nums text-foreground">
                    {mediumCount}
                  </span>
                  <button
                    type="button"
                    onClick={() => setMediumCount((prev) => Math.min(mediumInPool, prev + 1))}
                    disabled={mediumCount >= mediumInPool}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>

            {/* Hard Row */}
            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-rose-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">{t("quizSolver.difficulty.hard")}</span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {hardCount} / {hardInPool} {t("quizDetails.inBank")}
                </span>
                <div className="flex items-center gap-1.5">
                  <button
                    type="button"
                    onClick={() => setHardCount((prev) => Math.max(0, prev - 1))}
                    disabled={hardCount <= 0}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    -
                  </button>
                  <span className="w-6 text-center text-xs font-bold tabular-nums text-foreground">
                    {hardCount}
                  </span>
                  <button
                    type="button"
                    onClick={() => setHardCount((prev) => Math.min(hardInPool, prev + 1))}
                    disabled={hardCount >= hardInPool}
                    className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <SecondaryButton type="button" onClick={handleClose} disabled={isSubmitting}>
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton type="submit" disabled={isSubmitting}>
            {isSubmitting ? t("common.loading") : t("quizDetails.editSaveBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
