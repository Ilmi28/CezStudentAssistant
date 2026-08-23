import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useUser } from "../hooks";
import { PrimaryButton, SecondaryButton } from "../components/Button";
import { Select } from "../components/Select";
import MultiSegmentProgressBar from "../components/MultiSegmentProgressBar";
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



  const formattedSyncDate = lastCezSync
    ? new Date(lastCezSync).toLocaleString(i18n.language === "pl" ? "pl-PL" : "en-US", {
        dateStyle: "medium",
        timeStyle: "short",
      })
    : t("courses.syncSubtitleNoDate");

  return (
    <div className="max-w-4xl mx-auto space-y-8 animate-in fade-in duration-300">
      <div>
        <h1 className="text-2xl font-black text-foreground tracking-tight">
          {t("preferences.title")}
        </h1>
        <p className="text-sm text-muted-foreground mt-0.5 font-normal">
          {t("preferences.subtitle")}
        </p>
      </div>

      {/* Section 1: User Profile */}

      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.profileSection")}
          </h2>
        </div>

        <div className="flex items-center gap-4 py-1">
          <div className="w-12 h-12 rounded-full bg-primary/10 border border-primary/20 flex items-center justify-center text-primary font-bold text-lg">
            {username ? username.charAt(0).toUpperCase() : "U"}
          </div>
          <div>
            <div className="text-sm font-bold text-foreground">
              {username || t("preferences.defaultUsername")}
            </div>
            <div className="text-xs text-muted-foreground">
              {t("preferences.userRole")}
            </div>
          </div>
        </div>
      </section>

      {/* Section 2: App Preferences */}

      <section className="space-y-4">

        <div className="border-b border-border pb-2.5">
          <h2 className="text-base font-bold text-foreground tracking-wide">
            {t("preferences.appSection")}
          </h2>
        </div>

        {fetchingConfig ? (
          <div className="h-10 animate-pulse bg-muted rounded-xl" />
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

            <div>
              <label className="block text-xs font-semibold text-foreground mb-1.5">
                {t("preferences.themeLabel")}
              </label>

              <Select
                value={theme}
                onChange={(val) => handleThemeSelect(val as UserTheme)}
                options={[
                  { value: UserTheme.Dark, label: t("preferences.themeDark") },
                  { value: UserTheme.Light, label: t("preferences.themeLight") },
                ]}
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-foreground mb-1.5">
                {t("preferences.languageLabel")}
              </label>

              <Select
                value={language}
                onChange={(val) => handleLanguageSelect(val as UserLanguage)}
                options={[
                  { value: UserLanguage.Polish, label: t("preferences.langPolish") },
                  { value: UserLanguage.English, label: t("preferences.langEnglish") },
                ]}
              />
            </div>

          </div>
        )}

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
              {(() => {
                const limit = usage.dailyTokenLimit || 1;
                const realUsed = usage.dailyTokensUsed || 0;
                const reserved = usage.dailyTokensReserved || 0;
                const totalUsedAndReserved = realUsed + reserved;
                const totalPct = Math.min(100, Math.max(0, usage.dailyUsagePercentage));

                const realPctStr = limit > 0 ? ((realUsed / limit) * 100).toFixed(1) : "0";
                const reservedPctStr = limit > 0 ? ((reserved / limit) * 100).toFixed(1) : "0";
                const remainingTokens = Math.max(0, limit - totalUsedAndReserved);
                const remainingPctStr = limit > 0 ? ((remainingTokens / limit) * 100).toFixed(1) : "0";

                const segments = [
                  {
                    id: "real",
                    value: realUsed,
                    colorClass: "bg-sky-500",
                    customTooltip: `Zużyte: ${realUsed.toLocaleString()} (${realPctStr}%)`,
                  },
                  {
                    id: "reserved",
                    value: reserved,
                    colorClass: "bg-amber-500",
                    customTooltip: `Rezerwacje: ${reserved.toLocaleString()} (${reservedPctStr}%)`,
                  },
                ];

                return (
                  <div className="space-y-2">
                    <div className="flex justify-between items-baseline">
                      <span className="text-base font-bold text-primary">
                        {totalPct}%
                      </span>
                    </div>

                    <MultiSegmentProgressBar
                      segments={segments}
                      totalValue={limit}
                      heightClass="h-4"
                      showRemainingSegment
                      remainingSegmentTooltip={`Wolne: ${remainingTokens.toLocaleString()} (${remainingPctStr}%)`}
                    />
                  </div>
                );
              })()}
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
