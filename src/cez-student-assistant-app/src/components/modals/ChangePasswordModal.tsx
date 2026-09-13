import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "../ui/Input";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";

interface ChangePasswordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (currentPassword: string, newPassword: string) => Promise<void>;
}

export default function ChangePasswordModal({
  isOpen,
  onClose,
  onSubmit,
}: ChangePasswordModalProps) {
  const { t } = useTranslation();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentPassword || !newPassword || !confirmPassword) {
      setModalError(t("auth.emptyFields"));
      return;
    }

    if (newPassword !== confirmPassword) {
      setModalError(t("changePasswordModal.passwordsDoNotMatch"));
      return;
    }

    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(currentPassword, newPassword);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    setCurrentPassword("");
    setNewPassword("");
    setConfirmPassword("");
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("changePasswordModal.title")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-[12px] text-muted-foreground leading-relaxed">
          {t("changePasswordModal.desc")}
        </p>

        <Alert message={modalError} />

        <Input
          type="password"
          label={t("changePasswordModal.currentPassword")}
          value={currentPassword}
          onChange={(e) => {
            setCurrentPassword(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          type="password"
          label={t("changePasswordModal.newPassword")}
          value={newPassword}
          onChange={(e) => {
            setNewPassword(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          type="password"
          label={t("changePasswordModal.confirmPassword")}
          value={confirmPassword}
          onChange={(e) => {
            setConfirmPassword(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <div className="pt-2 flex gap-3">
          <SecondaryButton
            type="button"
            onClick={handleClose}
            className="flex-1"
          >
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            className="flex-1"
          >
            {t("changePasswordModal.submit")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
