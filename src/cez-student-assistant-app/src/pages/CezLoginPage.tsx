import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { BookOpen, ArrowLeft } from "lucide-react";
import { api } from "../services/api";

interface CezLoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function CezLoginPage({
  onLoginSuccess,
  setError,
  setSuccess
}: CezLoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [cezUser, setCezUser] = useState("");
  const [cezPass, setCezPass] = useState("");
  const [loading, setLoading] = useState(false);

  const handleCezLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cezUser || !cezPass) return setError(t("auth.emptyFields"));
    setLoading(true);
    try {
      await api.loginCez(cezUser, cezPass);
      onLoginSuccess(cezUser);
      setSuccess(t("auth.cezBtnSubmit") + " ✓");
    } catch (err: any) {
      setError(err.message || t("auth.emptyFields"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex-1 flex items-center justify-center p-6 sm:p-12 bg-background">
      <div className="w-full max-w-md bg-card rounded-xl border border-border shadow-md overflow-hidden animate-in fade-in duration-300">
        {/* Academic Header */}
        <div className="bg-sidebar px-8 py-8 text-center border-b border-sidebar-border">
          <div className="inline-flex w-12 h-12 rounded bg-primary border-2 border-white/20 items-center justify-center mb-3">
            <BookOpen size={22} className="text-white" />
          </div>
          <h1 style={{ fontFamily: "Roboto Slab, serif" }} className="text-white text-xl font-semibold leading-tight">
            CEZ Student Assistant
          </h1>
          <p className="text-[10px] tracking-[0.15em] uppercase text-white/55 mt-1 font-medium">
            Politechnika Białostocka
          </p>
        </div>

        <form onSubmit={handleCezLoginSubmit} className="p-8 space-y-5">
          <div>
            <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-semibold text-foreground mb-0.5 uppercase tracking-wide border-b border-border pb-2">
              {t("auth.cezTitle")}
            </h2>
            <p className="text-[11px] text-muted-foreground leading-relaxed mt-2 font-sans">
              {t("auth.cezSubtitle")}
            </p>
          </div>
          <div>
            <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-semibold mb-1.5 font-sans">
              {t("auth.cezUserLabel")}
            </label>
            <input
              type="text"
              value={cezUser}
              onChange={(e) => setCezUser(e.target.value)}
              className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary transition-colors font-mono"
              placeholder={t("auth.cezUserPlaceholder")}
            />
          </div>
          <div>
            <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-semibold mb-1.5 font-sans">{t("auth.cezPassLabel")}</label>
            <input
              type="password"
              value={cezPass}
              onChange={(e) => setCezPass(e.target.value)}
              className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary transition-colors font-mono"
              placeholder={t("auth.passwordPlaceholder")}
            />
          </div>
          <button
            type="submit"
            disabled={loading}
            className="w-full mt-2 bg-primary hover:bg-primary/90 disabled:bg-[#888] text-white py-2.5 rounded text-[13px] font-medium transition-colors shadow-sm cursor-pointer"
          >
            {loading ? t("auth.loginLoading") : t("auth.cezBtnSubmit")}
          </button>

          {/* Back Button */}
          <div className="pt-2">
            <button
              type="button"
              onClick={() => navigate("/login")}
              className="w-full flex items-center justify-center gap-1.5 text-[12px] text-primary hover:underline font-semibold cursor-pointer"
            >
              <ArrowLeft size={14} /> {t("auth.cezBack")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
