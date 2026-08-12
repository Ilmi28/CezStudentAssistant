import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse } from "../hooks";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import { userService } from "../services";
import { UserTheme, UserLanguage } from "../types";

export default function PreferencesPage() {
  const [theme, setThemeState] = useState<UserTheme>(UserTheme.Dark);
  const [language, setLanguageState] = useState<UserLanguage>(UserLanguage.Polish);
  const [fetchingConfig, setFetchingConfig] = useState(true);

  const { username, isCezConnected, lastCezSync, handleLogout } = useAuth();
  const { updateTheme, updateUserLanguage, setShowCezModal, syncing } = useUI();
  const { handleSyncCourses } = useCourse();
  const { t, i18n } = useTranslation();

  useEffect(() => {
    async function loadConfig() {
      try {
        const config = await userService.getUserConfiguration();
        setThemeState(config.theme);
        setLanguageState(config.language);
        const code = config.language === UserLanguage.English ? "en" : "pl";
        await i18n.changeLanguage(code);
      } catch {
        // Default fallbacks
      } finally {
        setFetchingConfig(false);
      }
    }
    loadConfig();
  }, [i18n]);

  const handleThemeSelect = (newTheme: UserTheme) => {
    setThemeState(newTheme);
    updateTheme(newTheme);
  };

  const handleLanguageSelect = async (newLang: UserLanguage) => {
    setLanguageState(newLang);
    const code = newLang === UserLanguage.English ? "en" : "pl";
    await i18n.changeLanguage(code);
    localStorage.setItem("language", code);
    await updateUserLanguage(newLang);
  };

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
        <h1 className="text-2xl font-bold text-foreground font-heading tracking-tight">
          {t("preferences.title")}
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("preferences.subtitle")}
        </p>
      </div>

      {/* Section 1: User Profile */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground font-heading tracking-wide">
            {t("preferences.accountSection")}
          </h2>
        </div>
        <div className="text-xs">
          <div>
            <span className="block text-muted-foreground text-[11px] font-mono uppercase tracking-wider mb-1">
              {t("auth.usernameLabel")}
            </span>
            <span className="font-mono text-sm text-foreground font-semibold">
              {username || "Student"}
            </span>
          </div>
        </div>
      </section>

      {/* Section 2: Appearance & Interface */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground font-heading tracking-wide">
            {t("preferences.appearanceSection")}
          </h2>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 text-xs">
          {/* Theme Selector */}
          <div className="space-y-1.5">
            <label htmlFor="themeSelect" className="block text-xs font-medium text-foreground">
              {t("preferences.themeLabel")}
            </label>
            <select
              id="themeSelect"
              value={theme}
              disabled={fetchingConfig}
              onChange={(e) => handleThemeSelect(Number(e.target.value) as UserTheme)}
              className="w-full px-3 py-2 rounded-lg bg-background text-foreground border border-border focus:border-primary focus:outline-hidden text-xs cursor-pointer shadow-xs"
            >
              <option value={UserTheme.Light}>{t("preferences.themeLight")}</option>
              <option value={UserTheme.Dark}>{t("preferences.themeDark")}</option>
              <option value={UserTheme.System}>{t("preferences.themeSystem")}</option>
            </select>
          </div>

          {/* Language Selector */}
          <div className="space-y-1.5">
            <label htmlFor="langSelect" className="block text-xs font-medium text-foreground">
              {t("preferences.languageLabel")}
            </label>
            <select
              id="langSelect"
              value={language}
              disabled={fetchingConfig}
              onChange={(e) => handleLanguageSelect(Number(e.target.value) as UserLanguage)}
              className="w-full px-3 py-2 rounded-lg bg-background text-foreground border border-border focus:border-primary focus:outline-hidden text-xs cursor-pointer shadow-xs"
            >
              <option value={UserLanguage.Polish}>Polski</option>
              <option value={UserLanguage.English}>English</option>
            </select>
          </div>
        </div>
      </section>

      {/* Section 3: CEZ Integration */}
      <section className="space-y-4">
        <div className="flex items-center justify-between border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground font-heading tracking-wide">
            {t("preferences.cezSection")}
          </h2>
          <span className={`px-2.5 py-0.5 rounded text-xs font-medium border ${
            isCezConnected
              ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20"
              : "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20"
          }`}>
            {isCezConnected ? t("preferences.connected") : t("preferences.notConnected")}
          </span>
        </div>
        <div className="space-y-3 text-xs">
          <p className="text-muted-foreground">
            {t("courses.syncSubtitleDate", { date: formattedSyncDate })}
          </p>
          <div>
            <SecondaryButton
              onClick={() => (isCezConnected ? handleSyncCourses() : setShowCezModal(true))}
              loading={syncing}
              size="sm"
              className="text-xs"
            >
              {isCezConnected ? t("preferences.syncNow") : t("courses.connectCezBtn")}
            </SecondaryButton>
          </div>
        </div>
      </section>

      {/* Section 4: Security & Session */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground font-heading tracking-wide">
            {t("preferences.securitySection")}
          </h2>
        </div>
        <div className="space-y-3 text-xs">
          <p className="text-muted-foreground">
            {t("preferences.logoutDesc")}
          </p>
          <div>
            <PrimaryButton
              onClick={handleLogout}
              size="sm"
              className="bg-destructive hover:bg-destructive/90 text-destructive-foreground border-none text-xs"
            >
              {t("common.logout")}
            </PrimaryButton>
          </div>
        </div>
      </section>
    </div>
  );
}
