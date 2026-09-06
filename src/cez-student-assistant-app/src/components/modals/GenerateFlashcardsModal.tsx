import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";
import DifficultyControlsGroup from "../ui/DifficultyControlsGroup";
import TokenEstimationWidget from "../ui/TokenEstimationWidget";
import { Toggle } from "../ui/Toggle";
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
    hardCount?: number | null,
    generateFromPromptOnly?: boolean
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
  const [promptOnlyMode, setPromptOnlyMode] = useState<boolean>(false);
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [estimation, setEstimation] = useState<EstimateFlashcardTokensDto | null>(null);
  const [loadingEstimation, setLoadingEstimation] = useState(false);

  const totalSelected = easyCount + mediumCount + hardCount;

  useEffect(() => {
    if (isOpen) {
      setPromptOnlyMode(false);
    }
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen || !courseId) {
      return;
    }

    if (!promptOnlyMode && !hasFiles) {
      setEstimation(null);
      return;
    }

    let isMounted = true;
    setLoadingEstimation(true);

    const timer = setTimeout(() => {
      flashcardService
        .estimateFlashcardTokens(courseId, totalSelected, additionalInstructions, promptOnlyMode)
        .then((result) => {
          if (isMounted) {
            setEstimation(result);
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
    }, 1000);

    return () => {
      isMounted = false;
      clearTimeout(timer);
    };
  }, [isOpen, hasFiles, courseId, promptOnlyMode, additionalInstructions, totalSelected]);

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (totalSelected <= 0) {
      setModalError("Musisz wybrać co najmniej 1 fiszkę.");
      return;
    }

    if (!promptOnlyMode && !hasFiles) {
      setModalError(t("courseDetails.noFilesWarning", "Brak aktywnych plików."));
      return;
    }

    if (promptOnlyMode && !additionalInstructions.trim()) {
      setModalError(t("auth.emptyFields"));
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
        hardCount,
        promptOnlyMode
      );
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("courseDetails.generateError"));
    } finally {
      setLoading(false);
    }
  };

  const isSubmitDisabled =
    loading ||
    (!promptOnlyMode && !hasFiles) ||
    (promptOnlyMode && !additionalInstructions.trim()) ||
    totalSelected <= 0 ||
    (estimation !== null && !estimation.canGenerate);

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={t("flashcards.generateModalTitle")}
      maxWidth="lg"
    >
      <form onSubmit={handleFormSubmit} className="space-y-4">
        {modalError && <Alert variant="error" message={modalError} />}

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

        <div className="space-y-2.5 pt-2 border-t border-border/60">
          <Toggle
            checked={promptOnlyMode}
            onChange={(checked) => {
              setPromptOnlyMode(checked);
              if (modalError) setModalError(null);
            }}
            label={t("courseDetails.promptOnlyToggleLabel", "Tylko własny prompt")}
          />

          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="block text-xs font-medium text-foreground">
                {t("courseDetails.generateInstructions")}
              </label>
              <span className="text-[11px] text-muted-foreground/70 font-medium tabular-nums">
                {additionalInstructions.length} / 5000
              </span>
            </div>
            <textarea
              value={additionalInstructions}
              onChange={(e) => {
                setAdditionalInstructions(e.target.value.slice(0, 5000));
                if (modalError) setModalError(null);
              }}
              rows={3}
              maxLength={5000}
              className="w-full px-3.5 py-2.5 rounded-xl bg-card border border-border text-foreground text-xs focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-colors resize-y min-h-[84px] max-h-[240px]"
            />
          </div>
        </div>

        {!promptOnlyMode && !hasFiles ? (
          <Alert
            variant="warning"
            message={t("courseDetails.noFilesWarning", "Brak aktywnych plików.")}
          />
        ) : (
          <TokenEstimationWidget
            hasFiles={promptOnlyMode ? true : hasFiles}
            loading={loadingEstimation}
            estimation={estimation}
            itemLabel={t("nav.flashcards")}
          />
        )}

        <div className="flex justify-end gap-3 pt-3 border-t border-border/60">
          <SecondaryButton type="button" onClick={onClose} disabled={loading}>
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            disabled={isSubmitDisabled}
          >
            {loading ? t("flashcards.generateBtnLoading") : t("flashcards.generateBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
