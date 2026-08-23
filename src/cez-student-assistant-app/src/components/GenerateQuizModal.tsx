import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";
import MultiSegmentProgressBar from "./MultiSegmentProgressBar";
import { courseService } from "../services/courseService";
import type { EstimateQuizTokensResponseDto } from "../types";

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
    questionCountPerAttempt?: number | null
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
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [baseEstimation, setBaseEstimation] = useState<EstimateQuizTokensResponseDto | null>(null);
  const [loadingEstimation, setLoadingEstimation] = useState(false);

  const totalSelected = easyCount + mediumCount + hardCount;
  const activeAttemptCount = questionCountPerAttempt != null
    ? Math.min(questionCountPerAttempt, Math.max(1, totalSelected))
    : Math.max(1, totalSelected);

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
    const count = Math.max(1, totalSelected);
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
        activeAttemptCount
      );
      setAdditionalInstructions("");
      setTimeLimitMinutes("");
      setEasyCount(2);
      setMediumCount(2);
      setHardCount(1);
      setQuestionCountPerAttempt(null);
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
    setEasyCount(2);
    setMediumCount(2);
    setHardCount(1);
    setQuestionCountPerAttempt(null);
    setBaseEstimation(null);
    onClose();
  };

  const isSubmitDisabled = !hasFiles || (currentEstimation !== null && !currentEstimation.canGenerate);

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("courseDetails.generateTitle")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        {!hasFiles && (
          <Alert message={t("courseDetails.generateNoFilesError")} variant="warning" />
        )}

        <p className="text-xs text-muted-foreground">
          {t("courseDetails.generateDesc")}
        </p>

        {/* Time Limit Section with Slider */}
        <div className="space-y-2.5">
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

        <div className="space-y-3.5 pt-2 border-t border-border/60">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-foreground font-medium">
              {t("quizDetails.difficultyDistribution", "Trudność")}
            </span>
            <span className="text-foreground font-bold text-xs bg-secondary px-2.5 py-0.5 rounded-full border border-border">
              {formatQuestionCount(totalSelected, i18n.language)}
            </span>
          </div>

          <MultiSegmentProgressBar
            segments={[
              { id: "easy", value: easyCount, colorClass: "bg-emerald-500", customTooltip: `${t("quizSolver.difficulty.easy", "Łatwe")} (${easyCount})` },
              { id: "medium", value: mediumCount, colorClass: "bg-amber-500", customTooltip: `${t("quizSolver.difficulty.medium", "Średnie")} (${mediumCount})` },
              { id: "hard", value: hardCount, colorClass: "bg-rose-500", customTooltip: `${t("quizSolver.difficulty.hard", "Trudne")} (${hardCount})` },
            ]}
            heightClass="h-3.5"
          />

          <div className="space-y-2">
            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.easy", "Łatwe")}
                </span>
              </div>
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
                  onClick={() => setEasyCount((prev) => Math.min(20, prev + 1))}
                  disabled={easyCount >= 20}
                  className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                >
                  +
                </button>
              </div>
            </div>

            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-amber-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.medium", "Średnie")}
                </span>
              </div>
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
                  onClick={() => setMediumCount((prev) => Math.min(20, prev + 1))}
                  disabled={mediumCount >= 20}
                  className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                >
                  +
                </button>
              </div>
            </div>

            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-rose-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.hard", "Trudne")}
                </span>
              </div>
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
                  onClick={() => setHardCount((prev) => Math.min(20, prev + 1))}
                  disabled={hardCount >= 20}
                  className="w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-primary/20 hover:border-primary/50 disabled:opacity-30 cursor-pointer text-sm font-bold transition-all"
                >
                  +
                </button>
              </div>
            </div>
          </div>
        </div>

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
                      currentEstimation.canGenerate ? "text-foreground" : "text-destructive"
                    }`}
                  >
                    ~{currentEstimation.estimatedDailyUsagePercentage}%
                  </span>
                </div>

                {(() => {
                  const limit = currentEstimation.dailyTokenLimit || 1;
                  const realUsed = currentEstimation.dailyTokensUsed || 0;
                  const otherReserved = currentEstimation.dailyTokensReserved || 0;
                  const thisQuizEstimated = currentEstimation.estimatedTokens || 0;
                  const remainingTokens = Math.max(0, limit - realUsed - otherReserved - thisQuizEstimated);

                  const realPctStr = limit > 0 ? ((realUsed / limit) * 100).toFixed(1) : "0";
                  const otherReservedPctStr = limit > 0 ? ((otherReserved / limit) * 100).toFixed(1) : "0";
                  const thisQuizPctStr = limit > 0 ? ((thisQuizEstimated / limit) * 100).toFixed(1) : "0";
                  const remainingPctStr = limit > 0 ? ((remainingTokens / limit) * 100).toFixed(1) : "0";

                  const segments = [
                    {
                      id: "real",
                      value: realUsed,
                      colorClass: "bg-sky-500",
                      customTooltip: `Zużyte: ${realUsed.toLocaleString()} (${realPctStr}%)`,
                    },
                    {
                      id: "other",
                      value: otherReserved,
                      colorClass: "bg-amber-500",
                      customTooltip: `Inne zlecenia: ${otherReserved.toLocaleString()} (${otherReservedPctStr}%)`,
                    },
                    {
                      id: "thisQuiz",
                      value: thisQuizEstimated,
                      colorClass: "bg-indigo-500",
                      customTooltip: `Ten quiz: ${thisQuizEstimated.toLocaleString()} (${thisQuizPctStr}%)`,
                    },
                  ];

                  return (
                    <MultiSegmentProgressBar
                      segments={segments}
                      totalValue={limit}
                      heightClass="h-3.5"
                      showRemainingSegment
                      remainingSegmentTooltip={`Wolne: ${remainingTokens.toLocaleString()} (${remainingPctStr}%)`}
                    />
                  );
                })()}
              </div>
            ) : null}
          </div>
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
