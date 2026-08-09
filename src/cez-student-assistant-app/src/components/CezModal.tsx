import { useState } from "react";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import { Input } from "./Input";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import cezLogo from "../assets/cez-logo.png";

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

  if (!isOpen) return null;

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
      setModalError(err.message || t("auth.emptyFields"));
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/45 backdrop-blur-xs">
      <div className="w-full max-w-md bg-card rounded-lg border border-border shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-150">
        {/* Modal header */}
        <div className="bg-sidebar px-6 py-4 flex items-center justify-between border-b border-sidebar-border">
          <div className="flex items-center gap-2 text-white">
            <div className="w-6 h-6 rounded-full bg-white p-0.5 flex items-center justify-center flex-shrink-0 shadow-xs">
              <img src={cezLogo} alt="CEZ" className="w-full h-full object-contain" />
            </div>
            <span style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-semibold">{t("cezModal.title")}</span>
          </div>
          <button onClick={() => { setModalError(null); onClose(); }} className="text-white/60 hover:text-white cursor-pointer">
            <X size={16} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
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
              size="sm"
              onClick={onClose}
              className="flex-1"
            >
              {t("cezModal.cancel")}
            </SecondaryButton>
            <PrimaryButton
              type="submit"
              size="sm"
              loading={loading}
              className="flex-1"
            >
              {t("cezModal.submit")}
            </PrimaryButton>
          </div>
        </form>
      </div>
    </div>
  );
}
