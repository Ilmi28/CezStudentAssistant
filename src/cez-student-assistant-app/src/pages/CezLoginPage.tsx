import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft } from "lucide-react";
import { authService } from "../services";
import { AuthLayout, Input, Alert, PrimaryButton, SecondaryButton, Heading } from "../components";

interface CezLoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
}

export default function CezLoginPage({
  onLoginSuccess
}: CezLoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [cezUser, setCezUser] = useState("");
  const [cezPass, setCezPass] = useState("");
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleCezLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cezUser.trim() || !cezPass) {
      setFormError(t("auth.emptyFields"));
      return;
    }

    setFormError(null);
    setLoading(true);
    try {
      await authService.loginCez(cezUser.trim(), cezPass);
      onLoginSuccess(cezUser.trim());
    } catch (err: any) {
      setFormError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleCezLoginSubmit} className="space-y-5">
        <div>
          <Heading level={2} size="sm" uppercase className="mb-0.5 border-b border-border pb-2">
            {t("auth.cezTitle")}
          </Heading>
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
