import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";
import DifficultyControlsGroup from "../ui/DifficultyControlsGroup";
import TokenEstimationWidget from "../ui/TokenEstimationWidget";
import { Toggle } from "../ui/Toggle";
import { courseService } from "../../services/courseService";
import type { EstimateQuizTokensResponseDto } from "../../types";

interface GenerateQuizModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (
    questionCount: number,
    timeLimitMinutes?: number | null,
    additionalInstructions?: string,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null,
    questionCountPerAttempt?: number | null,
    generateFromPromptOnly?: boolean
  ) => Promise<void>;
  hasFiles: boolean;
  courseId: string;
}

const formatQuestionCount = (n: number, lang: string) => {
  if (lang.startsWith("en")) {
    return n === 1 ? "1 question" : `${n} questions`;
  }
  if (n === 1) return "1 pytanie";
  if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) return `${n} pytania`;
  return `${n} pytań`;
};

export default function GenerateQuizModal({
  isOpen,
  onClose,
  onSubmit,
  hasFiles,
  courseId,
}: GenerateQuizModalProps) {
  const { t, i18n } = useTranslation();
  const [easyCount, setEasyCount] = useState<number>(2);
  const [mediumCount, setMediumCount] = useState<number>(2);
  const [hardCount, setHardCount] = useState<number>(1);
  const [questionCountPerAttempt, setQuestionCountPerAttempt] = useState<number | null>(null);
  const [timeLimitMinutes, setTimeLimitMinutes] = useState<number | "">("");
  const [additionalInstructions, setAdditionalInstructions] = useState("");
  const [promptOnlyMode, setPromptOnlyMode] = useState<boolean>(false);
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [estimation, setEstimation] = useState<EstimateQuizTokensResponseDto | null>(null);
  const [loadingEstimation, setLoadingEstimation] = useState(false);

  const totalSelected = easyCount + mediumCount + hardCount;
  const activeAttemptCount = questionCountPerAttempt != null
    ? Math.min(questionCountPerAttempt, Math.max(1, totalSelected))
    : Math.max(1, totalSelected);

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
      courseService
        .estimateQuizTokens(courseId, totalSelected, additionalInstructions, promptOnlyMode)
        .then((result) => {
          if (isMounted) {
            setEstimation(result);
          }
        })
        .catch((err) => {
          console.warn("[GenerateQuizModal] Baseline token estimation failed:", err);
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!promptOnlyMode && !hasFiles) {
      setModalError(t("courseDetails.generateNoFilesError", "Brak aktywnych plików."));
      return;
    }
    if (promptOnlyMode && !additionalInstructions.trim()) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    if (estimation && !estimation.canGenerate) {
      setModalError(t("courseDetails.tokenLimitExceededWarning"));
      return;
    }
    if (totalSelected <= 0) {
      setModalError(t("quizDetails.atLeastOneQuestionError", "Wybierz co najmniej 1 pytanie w podejściu."));
      return;
    }
    setModalError(null);
    setLoading(true);
    try {
      const limit = typeof timeLimitMinutes === "number" && timeLimitMinutes > 0 ? timeLimitMinutes : null;
      await onSubmit(
        totalSelected,
        limit,
        additionalInstructions.trim() || undefined,
        easyCount,
        mediumCount,
        hardCount,
        activeAttemptCount,
        promptOnlyMode
      );
      onClose();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setModalError(err.message || t("auth.genericError"));
      } else {
        setModalError(t("auth.genericError"));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    onClose();
  };

  const isSubmitDisabled =
    (!promptOnlyMode && !hasFiles) ||
    (promptOnlyMode && !additionalInstructions.trim()) ||
    (estimation !== null && !estimation.canGenerate);

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("courseDetails.generateTitle")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        {/* Time Limit Section with Slider */}
        <div className="space-y-2.5 pt-1">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-foreground font-medium">
              {t("quizDetails.quizTimeLimitLabel", "Limit czasu")}
            </span>
            <span
              className={`text-xs font-bold px-2.5 py-0.5 rounded-full border transition-colors ${
                timeLimitMinutes && Number(timeLimitMinutes) > 0
                  ? "bg-primary/10 text-primary border-primary/20"
                  : "bg-secondary text-muted-foreground border-border"
              }`}
            >
              {timeLimitMinutes && Number(timeLimitMinutes) > 0
                ? `${timeLimitMinutes} min`
                : t("quizDetails.stats.noLimit", "Brak limitu")}
            </span>
          </div>

          <input
            type="range"
            min={0}
            max={60}
            step={1}
            value={timeLimitMinutes === "" || timeLimitMinutes === null ? 0 : Number(timeLimitMinutes)}
            onChange={(e) => {
              const val = Number(e.target.value);
              setTimeLimitMinutes(val === 0 ? "" : val);
              if (modalError) setModalError(null);
            }}
            className="w-full h-2 bg-secondary rounded-lg appearance-none cursor-pointer accent-primary"
          />
        </div>

        {/* Questions Per Attempt Slider (1..totalSelected) */}
        <div className="space-y-2.5 pt-2 border-t border-border/60">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-foreground font-medium">
              {t("quizDetails.stats.questions", "W podejściu")}
            </span>
            <span className="text-foreground font-bold text-xs bg-secondary px-2.5 py-0.5 rounded-full border border-border">
              {formatQuestionCount(activeAttemptCount, i18n.language)}
            </span>
          </div>

          <input
            type="range"
            min={1}
            max={Math.max(1, totalSelected)}
            step={1}
            value={activeAttemptCount}
            onChange={(e) => {
              const val = Number(e.target.value);
              setQuestionCountPerAttempt(val);
              if (modalError) setModalError(null);
            }}
            className="w-full h-2 bg-secondary rounded-lg appearance-none cursor-pointer accent-primary"
          />
        </div>

        <DifficultyControlsGroup
          easyCount={easyCount}
          mediumCount={mediumCount}
          hardCount={hardCount}
          setEasyCount={setEasyCount}
          setMediumCount={setMediumCount}
          setHardCount={setHardCount}
          totalSelected={totalSelected}
          itemUnitLabel={formatQuestionCount(totalSelected, i18n.language).replace(/^\d+\s*/, "")}
          maxPerCategory={20}
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
                {t("courseDetails.generateInstructions", "Własne instrukcje")}
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
          <Alert message={t("courseDetails.generateNoFilesError", "Brak aktywnych plików.")} variant="warning" />
        ) : (
          <TokenEstimationWidget
            hasFiles={promptOnlyMode ? true : hasFiles}
            loading={loadingEstimation}
            estimation={estimation}
            itemLabel="Ten quiz"
          />
        )}

        <div className="flex justify-end gap-3 pt-2">
          <SecondaryButton type="button" onClick={handleClose}>
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            disabled={isSubmitDisabled}
          >
            {t("courseDetails.generateBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
