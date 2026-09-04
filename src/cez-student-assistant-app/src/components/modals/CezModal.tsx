import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "../ui/Input";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";
import cezLogo from "../../assets/cez-logo.png";

interface CezModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (cezUser: string, cezPass: string) => Promise<void>;
  loading: boolean;
}

export default function CezModal({
  isOpen,
  onClose,
  onSubmit,
  loading
}: CezModalProps) {
  const { t } = useTranslation();
  const [cezUser, setCezUser] = useState("");
  const [cezPass, setCezPass] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cezUser || !cezPass) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    setModalError(null);
    try {
      await onSubmit(cezUser, cezPass);
      setCezUser("");
      setCezPass("");
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
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
      title={t("cezModal.title")}
      icon={
        <div className="w-6 h-6 rounded-full bg-white p-0.5 flex items-center justify-center shrink-0 shadow-xs">
          <img src={cezLogo} alt="CEZ" className="w-full h-full object-contain" />
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-[12px] text-muted-foreground leading-relaxed">
          {t("cezModal.desc")}
        </p>

        <Alert message={modalError} />

        <Input
          label={t("cezModal.username")}
          value={cezUser}
          onChange={(e) => {
            setCezUser(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          type="password"
          label={t("cezModal.password")}
          value={cezPass}
          onChange={(e) => {
            setCezPass(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <div className="pt-2 flex gap-3">
          <SecondaryButton
            type="button"
            onClick={handleClose}
            className="flex-1"
          >
            {t("cezModal.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            className="flex-1"
          >
            {t("cezModal.submit")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
