import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authService } from "../services";
import { AuthLayout } from "../components/AuthLayout";
import { Input } from "../components/Input";
import { Alert } from "../components/Alert";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import cezLogo from "../assets/cez-logo.png";

interface LoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function LoginPage({
  onLoginSuccess,
  setSuccess
}: LoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [loginUser, setLoginUser] = useState("");
  const [loginPass, setLoginPass] = useState("");
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!loginUser || !loginPass) {
      setFormError(t("auth.emptyFields"));
      return;
    }
    setFormError(null);
    setLoading(true);
    try {
      await authService.login(loginUser, loginPass);
      onLoginSuccess(loginUser);
      setSuccess(t("auth.loginSuccess"));
    } catch (err: any) {
      setFormError(err.message || t("auth.emptyFields"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleLogin} className="space-y-4">
        <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-semibold text-foreground mb-2 uppercase tracking-wide border-b border-border pb-2">
          {t("auth.loginTitle")}
        </h2>

        <Alert message={formError} />

        <Input
          label={t("auth.usernameLabel")}
          value={loginUser}
          onChange={(e) => {
            setLoginUser(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <Input
          type="password"
          label={t("auth.passwordLabel")}
          value={loginPass}
          onChange={(e) => {
            setLoginPass(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <PrimaryButton type="submit" fullWidth loading={loading} className="mt-2">
          {t("auth.loginBtn")}
        </PrimaryButton>
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

      {/* CEZ Button */}
      <SecondaryButton
        type="button"
        fullWidth
        onClick={() => navigate("/login-cez")}
        icon={<img src={cezLogo} alt="CEZ" className="w-5 h-5 object-contain" />}
      >
        {t("auth.cezBtn")}
      </SecondaryButton>

      <div className="text-center pt-2">
        <span className="text-[12px] text-muted-foreground">{t("auth.noAccount")}</span>
        <button
          type="button"
          onClick={() => navigate("/register")}
          className="text-[12px] text-primary font-medium hover:underline focus:outline-none cursor-pointer"
        >
          {t("auth.registerLink")}
        </button>
      </div>
    </AuthLayout>
  );
}
