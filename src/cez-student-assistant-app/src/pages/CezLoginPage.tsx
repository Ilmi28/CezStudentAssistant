import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft } from "lucide-react";
import { authService } from "../services";
import { AuthLayout } from "../components/AuthLayout";
import { Input } from "../components/Input";
import { Alert } from "../components/Alert";
import { PrimaryButton, SecondaryButton } from "../components/Button";

interface CezLoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function CezLoginPage({
  onLoginSuccess,
  setSuccess
}: CezLoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [cezUser, setCezUser] = useState("");
  const [cezPass, setCezPass] = useState("");
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleCezLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cezUser || !cezPass) {
      setFormError(t("auth.emptyFields"));
      return;
    }
    setFormError(null);
    setLoading(true);
    try {
      await authService.loginCez(cezUser, cezPass);
      onLoginSuccess(cezUser);
      setSuccess(t("auth.cezSuccess"));
    } catch (err: any) {
      setFormError(err.message || t("auth.emptyFields"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleCezLoginSubmit} className="space-y-5">
        <div>
          <h2 style={{ fontFamily: "Roboto Slab, serif" }} className="text-base font-semibold text-foreground mb-0.5 uppercase tracking-wide border-b border-border pb-2">
            {t("auth.cezTitle")}
          </h2>
        </div>

        <Alert message={formError} />

        <Input
          label={t("auth.cezUserLabel")}
          value={cezUser}
          onChange={(e) => {
            setCezUser(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <Input
          type="password"
          label={t("auth.cezPassLabel")}
          value={cezPass}
          onChange={(e) => {
            setCezPass(e.target.value);
            if (formError) setFormError(null);
          }}
        />

        <PrimaryButton type="submit" fullWidth loading={loading} className="mt-2">
          {t("auth.cezBtnSubmit")}
        </PrimaryButton>

        {/* Back Button */}
        <div className="pt-2">
          <SecondaryButton
            type="button"
            fullWidth
            onClick={() => navigate("/login")}
            icon={<ArrowLeft size={14} />}
          >
            {t("auth.cezBack")}
          </SecondaryButton>
        </div>
      </form>
    </AuthLayout>
  );
}
