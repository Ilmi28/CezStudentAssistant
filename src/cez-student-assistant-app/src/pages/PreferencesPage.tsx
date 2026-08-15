import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useUser } from "../hooks";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import { Select } from "../components/Select";
import { userService } from "../services";
import { UserTheme, UserLanguage } from "../types";

export default function PreferencesPage() {
  const [theme, setThemeState] = useState<UserTheme>(UserTheme.Dark);
  const [language, setLanguageState] = useState<UserLanguage>(UserLanguage.Polish);
  const [fetchingConfig, setFetchingConfig] = useState(true);

  const { username, isCezConnected, lastCezSync, handleLogout } = useAuth();
  const { updateTheme, updateUserLanguage, setShowCezModal, syncing } = useUI();
  const { handleSyncCourses } = useCourse();
  const { usage, loadingUsage } = useUser();
  const { t, i18n } = useTranslation();


  useEffect(() => {
    async function loadConfig() {
      try {
        const config = await userService.getUserConfiguration();
        setThemeState(config.theme);
        setLanguageState(config.language);
        const code = config.language === UserLanguage.English ? "en" : "pl";
        await i18n.changeLanguage(code);
        localStorage.setItem("language", code);
      } catch (configErr) {
        console.debug("[PreferencesPage] Configuration load fallback:", configErr);
      } finally {
        setFetchingConfig(false);
      }
    }
    loadConfig();
  }, []);

  const handleThemeSelect = (newTheme: UserTheme) => {
    setThemeState(newTheme);
    updateTheme(newTheme);
  };

  const handleLanguageSelect = async (newLang: UserLanguage) => {
    setLanguageState(newLang);
    const code = newLang === UserLanguage.English ? "en" : "pl";
    localStorage.setItem("language", code);
    await i18n.changeLanguage(code);
    await updateUserLanguage(newLang);
  };

  const themeOptions = [
    { value: UserTheme.Light, label: t("preferences.themeLight") },
    { value: UserTheme.Dark, label: t("preferences.themeDark") },
    { value: UserTheme.System, label: t("preferences.themeSystem") },
  ];

  const languageOptions = [
    { value: UserLanguage.Polish, label: "Polski" },
    { value: UserLanguage.English, label: "English" },
  ];

  const formattedSyncDate = lastCezSync
    ? new Date(lastCezSync).toLocaleString(i18n.language === "pl" ? "pl-PL" : "en-US", {
        dateStyle: "medium",
        timeStyle: "short",
      })
    : t("courses.syncSubtitleNoDate");

  return (
    <div className="max-w-2xl mx-auto space-y-10 py-4 animate-in fade-in duration-150">
      {/* Main Page Title */}
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">
          {t("preferences.title")}
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("preferences.subtitle")}
        </p>
      </div>

      {/* Section 1: User Profile */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.accountSection")}
          </h2>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 text-sm">
          <div className="space-y-1.5">
            <label className="block text-sm font-medium text-foreground mb-1.5">
              {t("auth.usernameLabel")}
            </label>
            <div className="w-full px-4 py-3 rounded-xl bg-card border border-border text-foreground text-sm font-medium flex items-center justify-between shadow-xs">
              <span>{username || "Student"}</span>
            </div>
          </div>
        </div>
      </section>

      {/* Section 2: Appearance & Interface */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.appearanceSection")}
          </h2>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 text-sm">
          {/* Theme Selector */}
          <div className="space-y-1.5">
            <label htmlFor="themeSelect" className="block text-sm font-medium text-foreground mb-1.5">
              {t("preferences.themeLabel")}
            </label>
            <Select
              id="themeSelect"
              value={theme}
              options={themeOptions}
              disabled={fetchingConfig}
              onChange={handleThemeSelect}
            />
          </div>

          {/* Language Selector */}
          <div className="space-y-1.5">
            <label htmlFor="langSelect" className="block text-sm font-medium text-foreground mb-1.5">
              {t("preferences.languageLabel")}
            </label>
            <Select
              id="langSelect"
              value={language}
              options={languageOptions}
              disabled={fetchingConfig}
              onChange={handleLanguageSelect}
            />
          </div>
        </div>
      </section>

      {/* Section 3: Limit Dzienny */}
      <section className="space-y-4">

        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.usageSection")}
          </h2>
        </div>
        <div className="space-y-2 py-1">
          {loadingUsage ? (
            <div className="h-6 animate-pulse bg-muted rounded-lg" />
          ) : usage ? (
            <>
              <div className="flex justify-between items-baseline">
                <span className="text-base font-bold text-primary">
                  {usage.dailyUsagePercentage}%
                </span>
                <span className="text-sm font-medium text-foreground">
                  {t("preferences.tokensUsedFormat", {
                    used: usage.dailyTokensUsed.toLocaleString(),
                    limit: usage.dailyTokenLimit.toLocaleString(),
                  })}
                </span>
              </div>
              <div className="w-full bg-secondary h-2.5 rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary rounded-full transition-all duration-500 ease-out"
                  style={{ width: `${Math.min(100, Math.max(0, usage.dailyUsagePercentage))}%` }}
                />
              </div>
            </>
          ) : null}
        </div>

      </section>


      {/* Section 4: CEZ Integration */}

      <section className="space-y-4">
        <div className="flex items-center justify-between border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.cezSection")}
          </h2>
          <span className={`px-2.5 py-1 rounded-md text-xs font-semibold border ${
            isCezConnected
              ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20"
              : "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20"
          }`}>
            {isCezConnected ? t("preferences.connected") : t("preferences.notConnected")}
          </span>
        </div>
        <div className="space-y-3.5 text-sm">
          <p className="text-muted-foreground">
            {t("courses.syncSubtitleDate", { date: formattedSyncDate })}
          </p>
          <div>
            <SecondaryButton
              onClick={() => (isCezConnected ? handleSyncCourses() : setShowCezModal(true))}
              loading={syncing}
            >
              {isCezConnected ? t("preferences.syncNow") : t("courses.connectCezBtn")}
            </SecondaryButton>
          </div>
        </div>
      </section>

      {/* Section 4: Security & Session */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.securitySection")}
          </h2>
        </div>
        <div className="space-y-3.5 text-sm">
          <p className="text-muted-foreground">
            {t("preferences.logoutDesc")}
          </p>
          <div>
            <PrimaryButton
              onClick={handleLogout}
              className="bg-destructive hover:bg-destructive/90 text-destructive-foreground border-none"
            >
              {t("common.logout")}
            </PrimaryButton>
          </div>
        </div>
      </section>
    </div>
  );
}
