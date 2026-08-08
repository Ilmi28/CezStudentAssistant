import { useState } from "react";
import { useTranslation } from "react-i18next";
import { BookOpen, X } from "lucide-react";

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

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cezUser || !cezPass) return;
    await onSubmit(cezUser, cezPass);
    setCezUser("");
    setCezPass("");
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/45 backdrop-blur-xs">
      <div className="w-full max-w-md bg-card rounded-lg border border-border shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-150">
        {/* Modal header */}
        <div className="bg-sidebar px-6 py-4 flex items-center justify-between border-b border-sidebar-border">
          <div className="flex items-center gap-2 text-white">
            <BookOpen size={16} />
            <span style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-semibold">{t("cezModal.title")}</span>
          </div>
          <button onClick={onClose} className="text-white/60 hover:text-white cursor-pointer">
            <X size={16} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          <p className="text-[12px] text-muted-foreground leading-relaxed">
            {t("cezModal.desc")}
          </p>
          <div>
            <label className="block text-[10px] uppercase tracking-wider text-muted-foreground font-bold mb-1">
              {t("cezModal.username")}
            </label>
            <input
              type="text"
              value={cezUser}
              onChange={(e) => setCezUser(e.target.value)}
              className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary font-mono"
              placeholder="123456"
            />
          </div>
          <div>
            <label className="block text-[10px] uppercase tracking-wider text-muted-foreground font-bold mb-1">
              {t("cezModal.password")}
            </label>
            <input
              type="password"
              value={cezPass}
              onChange={(e) => setCezPass(e.target.value)}
              className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary font-mono"
              placeholder="••••••••"
            />
          </div>
          <div className="pt-2 flex gap-3">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 bg-muted hover:bg-muted/80 text-foreground py-2 rounded text-[12px] font-medium transition-colors cursor-pointer border border-border"
            >
              {t("cezModal.cancel")}
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex-1 bg-primary hover:bg-primary/90 text-white py-2 rounded text-[12px] font-medium transition-colors cursor-pointer shadow-sm"
            >
              {loading ? t("cezModal.submitLoading") : t("cezModal.submit")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
