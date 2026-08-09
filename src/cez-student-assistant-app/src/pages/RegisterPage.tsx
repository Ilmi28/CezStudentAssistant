import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { api } from "../services/api";
import { AuthLayout } from "../components/AuthLayout";
import { Input } from "../components/Input";
import { Alert } from "../components/Alert";
import { PrimaryButton } from "../components/Button";

interface RegisterPageProps {
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function RegisterPage({
  setSuccess
}: RegisterPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [regUser, setRegUser] = useState("");
  const [regPass, setRegPass] = useState("");
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!regUser || !regPass) {
      setFormError(t("auth.emptyFields"));
      return;
    }
    setFormError(null);
    setLoading(true);
    try {
      await api.register(regUser, regPass);
      setSuccess(t("auth.registerSuccess"));
      navigate("/login");
    } catch (err: any) {
      setFormError(err.message || t("auth.emptyFields"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleRegister} className="space-y-5">
        <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-semibold text-foreground mb-2 uppercase tracking-wide border-b border-border pb-2">
          {t("auth.registerTitle")}
        </h2>

        <Alert message={formError} />

        <Input
          label={t("auth.usernameLabel")}
          value={regUser}
          onChange={(e) => {
            setRegUser(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <Input
          type="password"
          label={t("auth.passwordLabel")}
          value={regPass}
          onChange={(e) => {
            setRegPass(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <PrimaryButton type="submit" fullWidth loading={loading} className="mt-2">
          {t("auth.registerBtn")}
        </PrimaryButton>

        <div className="text-center pt-2">
          <span className="text-[12px] text-muted-foreground">{t("auth.haveAccount")}</span>
          <button
            type="button"
            onClick={() => navigate("/login")}
            className="text-[12px] text-primary font-medium hover:underline focus:outline-none cursor-pointer"
          >
            {t("auth.loginLink")}
          </button>
        </div>
      </form>
    </AuthLayout>
  );
}
