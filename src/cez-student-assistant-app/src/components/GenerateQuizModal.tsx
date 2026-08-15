import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Brain, Minus, Plus } from "lucide-react";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";

interface GenerateQuizModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (questionCount: number, additionalInstructions?: string) => Promise<void>;
  hasFiles: boolean;
}

export default function GenerateQuizModal({
  isOpen,
  onClose,
  onSubmit,
  hasFiles,
}: GenerateQuizModalProps) {
  const { t } = useTranslation();
  const [questionCount, setQuestionCount] = useState<number>(5);
  const [additionalInstructions, setAdditionalInstructions] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!hasFiles) {
      setModalError(t("courseDetails.generateNoFilesError"));
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
      await onSubmit(count, additionalInstructions.trim() || undefined);
      setAdditionalInstructions("");
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    setAdditionalInstructions("");
    setQuestionCount(5);
    onClose();
  };

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
            {t("courseDetails.generateInstructions")}
          </label>
          <textarea
            value={additionalInstructions}
            onChange={(e) => setAdditionalInstructions(e.target.value)}
            rows={3}
            className="w-full px-3.5 py-2.5 rounded-xl bg-card border border-border text-foreground text-xs placeholder:text-muted-foreground/50 focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary transition-colors resize-none"
          />
        </div>

        <div className="pt-2 flex gap-3">
          <SecondaryButton type="button" onClick={handleClose} className="flex-1">
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            disabled={!hasFiles}
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
