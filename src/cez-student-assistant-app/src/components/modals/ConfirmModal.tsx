import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton, DangerButton } from "../ui/Button";
import { Flex } from "../ui/LayoutPrimitives";
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

        <Flex justify="end" gap={3} className="pt-2">
          <SecondaryButton
            type="button"
            onClick={handleClose}
          >
            {t("common.cancel")}
          </SecondaryButton>
          {isDestructive ? (
            <DangerButton
              type="button"
              onClick={handleConfirm}
              loading={loading}
            >
              {confirmBtnText || t("common.confirm")}
            </DangerButton>
          ) : (
            <PrimaryButton
              type="button"
              onClick={handleConfirm}
              loading={loading}
            >
              {confirmBtnText || t("common.confirm")}
            </PrimaryButton>
          )}
        </Flex>
      </div>
    </Modal>
  );
}
