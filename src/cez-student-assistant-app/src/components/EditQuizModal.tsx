import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "./Input";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";

interface EditQuizModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (displayName: string, timeLimitMinutes?: number | null) => Promise<void>;
  initialDisplayName: string;
  initialTimeLimitMinutes?: number | null;
}

export default function EditQuizModal({
  isOpen,
  onClose,
  onSubmit,
  initialDisplayName,
  initialTimeLimitMinutes,
}: EditQuizModalProps) {
  const { t } = useTranslation();
  const [displayName, setDisplayName] = useState(initialDisplayName || "");
  const [timeLimitMinutes, setTimeLimitMinutes] = useState<number | "">(
    initialTimeLimitMinutes ?? ""
  );
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setDisplayName(initialDisplayName || "");
      setTimeLimitMinutes(initialTimeLimitMinutes ?? "");
      setModalError(null);
    }
  }, [isOpen, initialDisplayName, initialTimeLimitMinutes]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!displayName.trim()) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    const limit =
      typeof timeLimitMinutes === "number" && timeLimitMinutes > 0
        ? timeLimitMinutes
        : null;

    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(displayName.trim(), limit);
      onClose();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setModalError(err.message || t("common.genericError"));
      } else {
        setModalError(t("common.genericError"));
      }
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
          value={displayName}
          onChange={(e) => {
            setDisplayName(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          type="number"
          min={1}
          max={300}
          label={t("quizDetails.quizTimeLimitLabel")}
          value={timeLimitMinutes}
          onChange={(e) => {
            const val = e.target.value === "" ? "" : parseInt(e.target.value, 10);
            setTimeLimitMinutes(val === "" || isNaN(val) ? "" : Math.max(1, val));
            if (modalError) setModalError(null);
          }}
        />

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
