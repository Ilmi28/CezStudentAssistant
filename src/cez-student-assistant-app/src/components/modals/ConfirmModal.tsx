import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";

interface ConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
  title: string;
  message: string;
  confirmBtnText?: string;
  isDestructive?: boolean;
}

export default function ConfirmModal({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmBtnText,
  isDestructive = true,
}: ConfirmModalProps) {
  const { t } = useTranslation();
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleConfirm = async () => {
    setModalError(null);
    setLoading(true);
    try {
      await onConfirm();
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={title}>
      <div className="space-y-4">
        <Alert message={modalError} />

        <p className="text-sm text-muted-foreground">{message}</p>

        <div className="pt-2 flex gap-3">
          <SecondaryButton
            type="button"
            onClick={handleClose}
            className="flex-1"
          >
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="button"
            onClick={handleConfirm}
            loading={loading}
            className={`flex-1 ${
              isDestructive
                ? "bg-destructive hover:bg-destructive/90 text-destructive-foreground border-none"
                : ""
            }`}
          >
            {confirmBtnText || t("common.confirm")}
          </PrimaryButton>
        </div>
      </div>
    </Modal>
  );
}
