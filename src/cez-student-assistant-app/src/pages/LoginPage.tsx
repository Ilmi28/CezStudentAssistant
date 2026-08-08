import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { BookOpen } from "lucide-react";
import { api } from "../services/api";

interface LoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function LoginPage({
  onLoginSuccess,
  setError,
  setSuccess
}: LoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [loginUser, setLoginUser] = useState("");
  const [loginPass, setLoginPass] = useState("");
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!loginUser || !loginPass) return setError(t("auth.emptyFields"));
    setLoading(true);
    try {
      await api.login(loginUser, loginPass);
      onLoginSuccess(loginUser);
      setSuccess(t("auth.loginBtn") + " ✓");
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

        <div className="p-8 space-y-5">
          <form onSubmit={handleLogin} className="space-y-4">
            <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-semibold text-foreground mb-2 uppercase tracking-wide border-b border-border pb-2">
              {t("auth.loginTitle")}
            </h2>
            <div>
              <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-medium mb-1.5 font-sans">{t("auth.usernameLabel")}</label>
              <input
                type="text"
                value={loginUser}
                onChange={(e) => setLoginUser(e.target.value)}
                className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary transition-colors font-mono"
                placeholder={t("auth.usernamePlaceholder")}
              />
            </div>
            <div>
              <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-medium mb-1.5 font-sans">{t("auth.passwordLabel")}</label>
              <input
                type="password"
                value={loginPass}
                onChange={(e) => setLoginPass(e.target.value)}
                className="w-full px-3 py-2 text-[13px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary transition-colors font-mono"
                placeholder={t("auth.passwordPlaceholder")}
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="w-full mt-2 bg-primary hover:bg-primary/90 disabled:bg-[#888] text-white py-2.5 rounded text-[13px] font-medium transition-colors shadow-sm cursor-pointer"
            >
              {loading ? t("auth.loginLoading") : t("auth.loginBtn")}
            </button>
          </form>

          {/* Separator */}
          <div className="relative flex items-center justify-center my-4">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-border"></div>
            </div>
            <span className="relative px-3 bg-card text-[10px] uppercase tracking-wider text-muted-foreground font-semibold">
              {t("auth.orText")}
            </span>
          </div>

          {/* CEZ PB Button */}
          <button
            type="button"
            onClick={() => navigate("/login-cez")}
            className="w-full flex items-center justify-center gap-3 px-4 py-2.5 rounded border border-border hover:border-primary/30 hover:bg-muted/50 text-[13px] font-semibold transition-colors shadow-xs cursor-pointer text-foreground"
          >
            <div className="w-6 h-6 rounded bg-primary flex items-center justify-center text-white flex-shrink-0">
              <BookOpen size={13} />
            </div>
            <span>{t("auth.cezBtn")}</span>
          </button>

          <div className="text-center pt-2">
            <span className="text-[12px] text-muted-foreground">{t("auth.noAccount")}</span>
            <button
              type="button"
              onClick={() => navigate("/register")}
              className="text-[12px] text-primary font-medium hover:underline focus:outline-none"
            >
              {t("auth.registerLink")}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
