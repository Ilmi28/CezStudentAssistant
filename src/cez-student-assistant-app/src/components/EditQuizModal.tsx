import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "./Input";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";
import MultiSegmentProgressBar from "./MultiSegmentProgressBar";

interface EditQuizModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (
    name: string,
    timeLimitMinutes?: number | null,
    questionCountPerAttempt?: number | null,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ) => Promise<void>;
  initialName: string;
  initialTimeLimitMinutes?: number | null;
  initialQuestionCountPerAttempt?: number | null;
  initialEasyCount?: number | null;
  initialMediumCount?: number | null;
  initialHardCount?: number | null;
  easyInPool: number;
  mediumInPool: number;
  hardInPool: number;
}

const formatQuestionCount = (n: number, lang: string) => {
  if (lang.startsWith("en")) {
    return n === 1 ? "1 question" : `${n} questions`;
  }
  if (n === 1) return "1 pytanie";
  if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) return `${n} pytania`;
  return `${n} pytań`;
};

export default function EditQuizModal({
  isOpen,
  onClose,
  onSubmit,
  initialName,
  initialTimeLimitMinutes,
  initialQuestionCountPerAttempt,
  initialEasyCount,
  initialMediumCount,
  initialHardCount,
  easyInPool,
  mediumInPool,
  hardInPool,
}: EditQuizModalProps) {
  const { t, i18n } = useTranslation();
  const [name, setName] = useState(initialName || "");
  const [timeLimitMinutes, setTimeLimitMinutes] = useState<number | "">(
    initialTimeLimitMinutes ?? ""
  );

  const [easyCount, setEasyCount] = useState<number>(
    initialEasyCount ?? Math.min(2, easyInPool)
  );
  const [mediumCount, setMediumCount] = useState<number>(
    initialMediumCount ?? Math.min(2, mediumInPool)
  );
  const [hardCount, setHardCount] = useState<number>(
    initialHardCount ?? Math.min(1, hardInPool)
  );

  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setName(initialName || "");
      setTimeLimitMinutes(initialTimeLimitMinutes ?? "");
      setEasyCount(initialEasyCount ?? Math.min(2, easyInPool));
      setMediumCount(initialMediumCount ?? Math.min(2, mediumInPool));
      setHardCount(initialHardCount ?? Math.min(1, hardInPool));
      setModalError(null);
    }
  }, [
    isOpen,
    initialName,
    initialTimeLimitMinutes,
    initialQuestionCountPerAttempt,
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
      setModalError(t("auth.emptyFields"));
      return;
    }
    const limit =
      typeof timeLimitMinutes === "number" && timeLimitMinutes > 0
        ? timeLimitMinutes
        : null;

    if (totalSelected <= 0) {
      setModalError(t("quizDetails.atLeastOneQuestionError", "Wybierz co najmniej 1 pytanie w podejściu."));
      return;
    }

    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(
        name.trim(),
        limit,
        totalSelected,
        easyCount,
        mediumCount,
        hardCount
      );
      onClose();
    } catch (err: unknown) {
      console.warn("[EditQuizModal] Failed to update quiz:", err);
      setModalError(t("common.genericError"));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("quizDetails.editModalTitle")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        <Input
          label={t("quizDetails.quizNameLabel")}
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        {/* Time Limit Section with Slider & Presets */}
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

          {/* Time Limit Slider (0 = Brak limitu, 1..60 = minuty) */}
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

        {/* Difficulty Distribution Section */}
        <div className="space-y-3.5 pt-3 border-t border-border/60">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-foreground font-medium">
              {t("quizDetails.difficultyDistribution", "Rozkład trudności pytań")}
            </span>
            <span className="text-foreground font-bold text-xs bg-secondary px-2.5 py-0.5 rounded-full border border-border">
              {formatQuestionCount(totalSelected, i18n.language)}
            </span>
          </div>

          {/* Segmented Live Preview Bar */}
          <MultiSegmentProgressBar
            segments={[
              { id: "easy", value: easyCount, colorClass: "bg-emerald-500", customTooltip: `${t("quizSolver.difficulty.easy", "Łatwe")} (${easyCount})` },
              { id: "medium", value: mediumCount, colorClass: "bg-amber-500", customTooltip: `${t("quizSolver.difficulty.medium", "Średnie")} (${mediumCount})` },
              { id: "hard", value: hardCount, colorClass: "bg-rose-500", customTooltip: `${t("quizSolver.difficulty.hard", "Trudne")} (${hardCount})` },
            ]}
            heightClass="h-3.5"
          />

          {/* 3 Spacious Control Rows */}
          <div className="space-y-2">
            {/* Easy Row */}
            <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 shadow-sm shrink-0" />
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.easy", "Łatwe")}
                </span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {easyCount} / {easyInPool} {t("quizDetails.inBank", "w bazie")}
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
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.medium", "Średnie")}
                </span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {mediumCount} / {mediumInPool} {t("quizDetails.inBank", "w bazie")}
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
                <span className="text-xs font-semibold text-foreground">
                  {t("quizSolver.difficulty.hard", "Trudne")}
                </span>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-[11px] font-medium text-muted-foreground tabular-nums">
                  {hardCount} / {hardInPool} {t("quizDetails.inBank", "w bazie")}
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

        <div className="pt-2 flex gap-3">
          <SecondaryButton type="button" onClick={handleClose} className="flex-1">
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton type="submit" loading={loading} className="flex-1">
            {t("quizDetails.editSaveBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
