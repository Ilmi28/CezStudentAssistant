import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authService } from "../services";
import { AuthLayout, Input, Alert, PrimaryButton, SecondaryButton, Heading, Text, Flex } from "../components";
import cezLogo from "../assets/cez-logo.png";

interface LoginPageProps {
  onLoginSuccess: (username: string) => void;
  setError: (msg: string) => void;
}

export default function LoginPage({
  onLoginSuccess
}: LoginPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [loginUser, setLoginUser] = useState("");
  const [loginPass, setLoginPass] = useState("");
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!loginUser.trim() || !loginPass) {
      setFormError(t("auth.emptyFields"));
      return;
    }

    setFormError(null);
    setLoading(true);
    try {
      await authService.login(loginUser.trim(), loginPass);
      onLoginSuccess(loginUser.trim());
    } catch (err: any) {
      setFormError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout>
      <form onSubmit={handleLogin} className="space-y-4">
        <Heading level={2} size="sm" uppercase className="mb-2 border-b border-border pb-2">
          {t("auth.loginTitle")}
        </Heading>

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

      <Flex align="center" justify="center" className="relative my-4">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-border"></div>
        </div>
        <Text size="xs" variant="subtle" uppercase className="relative px-3 bg-card font-semibold tracking-wider">
          {t("auth.orText")}
        </Text>
      </Flex>

      <SecondaryButton
        type="button"
        fullWidth
        onClick={() => navigate("/login-cez")}
        icon={<img src={cezLogo} alt="CEZ" className="w-5 h-5 object-contain" />}
      >
        {t("auth.cezBtn")}
      </SecondaryButton>

      <Flex align="center" justify="center" gap={1} className="pt-2">
        <Text size="xs" variant="muted">{t("auth.noAccount")}</Text>
        <button
          type="button"
          onClick={() => navigate("/register")}
          className="text-xs text-primary font-medium hover:underline focus:outline-none cursor-pointer transition-colors"
        >
          {t("auth.registerLink")}
        </button>
      </Flex>
    </AuthLayout>
  );
}
