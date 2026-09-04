import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";
import DifficultyControlsGroup from "../ui/DifficultyControlsGroup";
import TokenEstimationWidget from "../ui/TokenEstimationWidget";
import { flashcardService } from "../../services/flashcardService";
import type { EstimateFlashcardTokensDto } from "../../types/flashcardTypes";

interface GenerateFlashcardsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (
    cardCount: number,
    additionalInstructions?: string,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ) => Promise<void>;
  hasFiles: boolean;
  courseId: string;
}

const formatCardCount = (n: number) => {
  if (n === 1) return "1 fiszka";
  if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) return `${n} fiszki`;
  return `${n} fiszek`;
};

export default function GenerateFlashcardsModal({
  isOpen,
  onClose,
  onSubmit,
  hasFiles,
  courseId,
}: GenerateFlashcardsModalProps) {
  const { t } = useTranslation();
  const [easyCount, setEasyCount] = useState<number>(4);
  const [mediumCount, setMediumCount] = useState<number>(4);
  const [hardCount, setHardCount] = useState<number>(2);
  const [additionalInstructions, setAdditionalInstructions] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [baseEstimation, setBaseEstimation] = useState<EstimateFlashcardTokensDto | null>(null);
  const [loadingEstimation, setLoadingEstimation] = useState(false);

  const totalSelected = easyCount + mediumCount + hardCount;

  useEffect(() => {
    if (!isOpen || !hasFiles || !courseId) {
      setBaseEstimation(null);
      setLoadingEstimation(false);
      return;
    }

    let isMounted = true;
    setLoadingEstimation(true);

    flashcardService
      .estimateFlashcardTokens(courseId, 10)
      .then((result) => {
        if (isMounted) {
          setBaseEstimation(result);
        }
      })
      .catch((err) => {
        console.warn("[GenerateFlashcardsModal] Token estimation failed:", err);
      })
      .finally(() => {
        if (isMounted) {
          setLoadingEstimation(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [isOpen, hasFiles, courseId]);

  const currentEstimation = (() => {
    if (!baseEstimation) return null;
    const count = Math.max(1, totalSelected);
    const baseInputTokens = Math.max(0, baseEstimation.estimatedTokens - 1500);
    const totalEstimatedTokens = baseInputTokens + count * 150;
    const estimatedPercentage = baseEstimation.dailyTokenLimit > 0
      ? Math.round((totalEstimatedTokens / baseEstimation.dailyTokenLimit) * 10000) / 100
      : 0;
    const canGenerate = (baseEstimation.dailyTokensUsed + totalEstimatedTokens) <= baseEstimation.dailyTokenLimit;

    return {
      estimatedTokens: totalEstimatedTokens,
      estimatedDailyUsagePercentage: estimatedPercentage,
      canGenerate,
      dailyTokenLimit: baseEstimation.dailyTokenLimit,
      dailyTokensUsed: baseEstimation.dailyTokensUsed,
      dailyTokensReserved: baseEstimation.dailyTokensReserved,
    };
  })();

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (totalSelected <= 0) {
      setModalError("Musisz wybrać co najmniej 1 fiszkę.");
      return;
    }

    if (!hasFiles) {
      setModalError(t("courseDetails.noFilesWarning"));
      return;
    }

    setLoading(true);
    setModalError(null);

    try {
      await onSubmit(
        totalSelected,
        additionalInstructions.trim() || undefined,
        easyCount,
        mediumCount,
        hardCount
      );
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("courseDetails.generateError"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={t("flashcards.generateModalTitle")}
      maxWidth="lg"
    >
      <form onSubmit={handleFormSubmit} className="space-y-4">
        <p className="text-xs text-muted-foreground leading-relaxed -mt-1 mb-2">
          {t("flashcards.generateModalDesc")}
        </p>

        {modalError && <Alert variant="error" message={modalError} />}

        {!hasFiles && (
          <Alert
            variant="warning"
            message={t("courseDetails.noFilesWarning")}
          />
        )}

        <DifficultyControlsGroup
          easyCount={easyCount}
          mediumCount={mediumCount}
          hardCount={hardCount}
          setEasyCount={setEasyCount}
          setMediumCount={setMediumCount}
          setHardCount={setHardCount}
          totalSelected={totalSelected}
          itemUnitLabel={formatCardCount(totalSelected).replace(/^\d+\s*/, "")}
          maxPerCategory={30}
        />

        <div>
          <label className="block text-xs font-medium text-foreground mb-1.5">
            {t("courseDetails.generateInstructions")}
          </label>
          <textarea
            value={additionalInstructions}
            onChange={(e) => setAdditionalInstructions(e.target.value)}
            rows={2}
            className="w-full px-3.5 py-2 rounded-xl bg-card border border-border text-foreground text-xs focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-colors resize-none"
          />
        </div>

        <TokenEstimationWidget
          hasFiles={hasFiles}
          loading={loadingEstimation}
          estimation={currentEstimation}
          itemLabel={t("nav.flashcards")}
        />

        <div className="flex justify-end gap-3 pt-3 border-t border-border/60">
          <SecondaryButton type="button" onClick={onClose} disabled={loading}>
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            disabled={
              loading ||
              !hasFiles ||
              totalSelected <= 0 ||
              (currentEstimation !== null && !currentEstimation.canGenerate)
            }
          >
            {loading ? t("flashcards.generateBtnLoading") : t("flashcards.generateBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
