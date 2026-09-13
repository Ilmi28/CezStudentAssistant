import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "../ui/Input";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";

interface EditProfileModalProps {
  isOpen: boolean;
  onClose: () => void;
  initialUserName: string;
  initialFullName?: string | null;
  initialEmail?: string | null;
  onSubmit: (userName: string, fullName?: string, email?: string) => Promise<void>;
}

export default function EditProfileModal({
  isOpen,
  onClose,
  initialUserName,
  initialFullName,
  initialEmail,
  onSubmit,
}: EditProfileModalProps) {
  const { t } = useTranslation();
  const [userName, setUserName] = useState(initialUserName || "");
  const [fullName, setFullName] = useState(initialFullName || "");
  const [email, setEmail] = useState(initialEmail || "");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setUserName(initialUserName || "");
      setFullName(initialFullName || "");
      setEmail(initialEmail || "");
      setModalError(null);
    }
  }, [isOpen, initialUserName, initialFullName, initialEmail]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userName.trim()) {
      setModalError(t("editProfileModal.emptyUsernameError", "Nazwa użytkownika jest wymagana."));
      return;
    }

    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(userName.trim(), fullName.trim(), email.trim());
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("common.errorGeneric", "Wystąpił błąd podczas zapisywania profilu."));
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
      title={t("editProfileModal.title", "Edycja Profilu")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-[12px] text-muted-foreground leading-relaxed">
          {t("editProfileModal.desc", "Zaktualizuj swoje dane profilu lokalnego w aplikacji.")}
        </p>

        <Alert message={modalError} />

        <Input
          label={t("editProfileModal.username", "Nazwa użytkownika")}
          value={userName}
          onChange={(e) => {
            setUserName(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          label={t("editProfileModal.fullName", "Imię i nazwisko")}
          value={fullName}
          onChange={(e) => {
            setFullName(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          type="email"
          label={t("editProfileModal.email", "Adres e-mail")}
          value={email}
          onChange={(e) => {
            setEmail(e.target.value);
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
            {t("common.save", "Zapisz")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
