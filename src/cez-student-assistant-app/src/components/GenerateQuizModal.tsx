import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Brain, Minus, Plus } from "lucide-react";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";
import { courseService } from "../services/courseService";
import type { EstimateQuizTokensResponseDto } from "../types";

interface GenerateQuizModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (questionCount: number, timeLimitMinutes?: number, additionalInstructions?: string) => Promise<void>;
  hasFiles: boolean;
  courseId: string;
}

export default function GenerateQuizModal({
  isOpen,
  onClose,
  onSubmit,
  hasFiles,
  courseId,
}: GenerateQuizModalProps) {
  const { t } = useTranslation();
  const [questionCount, setQuestionCount] = useState<number>(5);
  const [timeLimitMinutes, setTimeLimitMinutes] = useState<number | "">("");
  const [additionalInstructions, setAdditionalInstructions] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [baseEstimation, setBaseEstimation] = useState<EstimateQuizTokensResponseDto | null>(null);
  const [loadingEstimation, setLoadingEstimation] = useState(false);

  useEffect(() => {
    if (!isOpen || !hasFiles || !courseId) {
      setBaseEstimation(null);
      setLoadingEstimation(false);
      return;
    }

    let isMounted = true;
    setLoadingEstimation(true);

    courseService
      .estimateQuizTokens(courseId, 5)
      .then((result) => {
        if (isMounted) {
          setBaseEstimation(result);
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

    return () => {
      isMounted = false;
    };
  }, [isOpen, hasFiles, courseId]);

  const currentEstimation = (() => {
    if (!baseEstimation) return null;
    const count = Math.max(1, questionCount || 5);
    const baseInputTokens = Math.max(0, baseEstimation.estimatedTokens - 1000);
    const totalEstimatedTokens = baseInputTokens + count * 200;
    const estimatedPercentage = baseEstimation.dailyTokenLimit > 0
      ? Math.round((totalEstimatedTokens / baseEstimation.dailyTokenLimit) * 10000) / 100
      : 0;
    const canGenerate = (baseEstimation.dailyTokensUsed + totalEstimatedTokens) <= baseEstimation.dailyTokenLimit;

    return {
      ...baseEstimation,
      estimatedTokens: totalEstimatedTokens,
      estimatedDailyUsagePercentage: estimatedPercentage,
      canGenerate,
    };
  })();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!hasFiles) {
      setModalError(t("courseDetails.generateNoFilesError"));
      return;
    }
    if (currentEstimation && !currentEstimation.canGenerate) {
      setModalError(t("courseDetails.tokenLimitExceededWarning"));
      return;
    }
    const count = Number(questionCount);
    if (!count || count < 1) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    setModalError(null);
    setLoading(true);
    try {
      const limit = typeof timeLimitMinutes === "number" && timeLimitMinutes > 0 ? timeLimitMinutes : undefined;
      await onSubmit(count, limit, additionalInstructions.trim() || undefined);
      setAdditionalInstructions("");
      setTimeLimitMinutes("");
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
    setAdditionalInstructions("");
    setTimeLimitMinutes("");
    setQuestionCount(5);
    setBaseEstimation(null);
    onClose();
  };

  const isSubmitDisabled = !hasFiles || (currentEstimation !== null && !currentEstimation.canGenerate);

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("courseDetails.generateTitle")}
      icon={<Brain size={20} className="text-primary" />}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        {!hasFiles && (
          <Alert message={t("courseDetails.generateNoFilesError")} variant="warning" />
        )}

        {currentEstimation && !currentEstimation.canGenerate && (
          <Alert message={t("courseDetails.tokenLimitExceededWarning")} variant="warning" />
        )}

        <p className="text-xs text-muted-foreground">
          {t("courseDetails.generateDesc")}
        </p>

        <div className="w-full space-y-1.5">
          <label className="block text-xs font-medium text-foreground">
            {t("courseDetails.generateQuestionsCount")}
          </label>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setQuestionCount((prev) => Math.max(1, prev - 1))}
              className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors cursor-pointer shrink-0"
              title="Decrease"
            >
              <Minus size={16} />
            </button>
            <input
              type="number"
              min={1}
              max={50}
              value={questionCount || ""}
              onChange={(e) => {
                const val = parseInt(e.target.value, 10);
                setQuestionCount(isNaN(val) ? 0 : val);
                if (modalError) setModalError(null);
              }}
              className="w-full h-10 px-4 text-center text-sm font-semibold rounded-xl bg-card border border-border text-foreground transition-all focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
            />
            <button
              type="button"
              onClick={() => setQuestionCount((prev) => Math.min(50, prev + 1))}
              className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors cursor-pointer shrink-0"
              title="Increase"
            >
              <Plus size={16} />
            </button>
          </div>
        </div>

        <div>
          <label className="block text-xs font-medium text-foreground mb-1.5">
            {t("courseDetails.generateTimeLimit")}
          </label>
          <input
            type="number"
            min={1}
            max={300}
            value={timeLimitMinutes}
            onChange={(e) => {
              const val = e.target.value === "" ? "" : parseInt(e.target.value, 10);
              setTimeLimitMinutes(val === "" || isNaN(val) ? "" : Math.max(1, val));
              if (modalError) setModalError(null);
            }}
            className="w-full h-10 px-3.5 rounded-xl bg-card border border-border text-foreground text-xs focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-colors [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
          />
        </div>

        <div>
          <label className="block text-xs font-medium text-foreground mb-1.5">
            {t("courseDetails.generateInstructions")}
          </label>
          <textarea
            value={additionalInstructions}
            onChange={(e) => setAdditionalInstructions(e.target.value)}
            rows={3}
            className="w-full px-3.5 py-2.5 rounded-xl bg-card border border-border text-foreground text-xs focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-colors resize-none"
          />
        </div>

        {hasFiles && (
          <div className="space-y-2 pt-1">
            <span className="block text-xs font-semibold text-foreground tracking-wide">
              {t("courseDetails.estimatedUsageTitle")}
            </span>

            {loadingEstimation ? (
              <div className="h-6 animate-pulse bg-muted rounded-lg" />
            ) : currentEstimation ? (
              <div className="space-y-1.5">
                <div className="flex justify-between items-baseline">
                  <span
                    className={`text-sm font-bold ${
                      currentEstimation.canGenerate ? "text-primary" : "text-destructive"
                    }`}
                  >
                    ~{currentEstimation.estimatedDailyUsagePercentage}%
                  </span>
                  <span className="text-xs font-medium text-foreground">
                    ~{currentEstimation.estimatedTokens.toLocaleString()} {t("courseDetails.tokensUnit")}
                  </span>
                </div>
                <div className="w-full bg-secondary h-2.5 rounded-full overflow-hidden flex">
                  {currentEstimation.dailyTokenLimit > 0 && currentEstimation.dailyTokensUsed > 0 && (
                    <div
                      className="h-full bg-primary/35 transition-all duration-300"
                      style={{
                        width: `${Math.min(
                          100,
                          (currentEstimation.dailyTokensUsed / currentEstimation.dailyTokenLimit) * 100
                        )}%`,
                      }}
                      title={`${t("preferences.dailyUsage")}: ${currentEstimation.dailyTokensUsed.toLocaleString()}`}
                    />
                  )}
                  {currentEstimation.dailyTokenLimit > 0 && (
                    <div
                      className={`h-full transition-all duration-300 ${
                        currentEstimation.canGenerate ? "bg-sky-500" : "bg-destructive"
                      }`}
                      style={{
                        width: `${Math.min(
                          100,
                          (currentEstimation.estimatedTokens / currentEstimation.dailyTokenLimit) * 100
                        )}%`,
                      }}
                      title={`${t("courseDetails.estimatedUsageTitle")}: ${currentEstimation.estimatedTokens.toLocaleString()}`}
                    />
                  )}
                </div>
              </div>
            ) : (
              <div className="text-xs text-muted-foreground">—</div>
            )}
          </div>
        )}

        <div className="pt-2 flex gap-3">
          <SecondaryButton type="button" onClick={handleClose} className="flex-1">
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            disabled={isSubmitDisabled}
            icon={!loading ? <Brain size={16} /> : undefined}
            className="flex-1"
          >
            {t("courseDetails.generateBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
