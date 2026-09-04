import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useUser } from "../hooks";
import {
  SecondaryButton,
  Select,
  MultiSegmentProgressBar,
  Heading,
  Text,
  Flex,
  Grid,
  Badge,
} from "../components";
import { userService } from "../services";
import { UserTheme, UserLanguage } from "../types";

export default function PreferencesPage() {
  const [theme, setThemeState] = useState<UserTheme>(UserTheme.Dark);
  const [language, setLanguageState] = useState<UserLanguage>(UserLanguage.Polish);
  const [fetchingConfig, setFetchingConfig] = useState(true);

  const { username, isCezConnected, lastCezSync, handleLogout, setIsCezConnected, setLastCezSync } = useAuth();
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
        setIsCezConnected(config.isCezConnected);
        if (config.lastCezSync !== undefined) {
          setLastCezSync(config.lastCezSync);
        }
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
        <Heading level={1} size="2xl" className="font-black tracking-tight">
          {t("preferences.title")}
        </Heading>
        <Text size="sm" variant="muted" className="mt-0.5 font-normal">
          {t("preferences.subtitle")}
        </Text>
      </div>

      {/* Section 1: User Profile */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.profileSection")}
          </Heading>
        </div>

        <Flex align="center" gap={4} className="py-1">
          <div className="w-12 h-12 rounded-full bg-primary/10 border border-primary/20 flex items-center justify-center text-primary font-bold text-lg">
            {username ? username.charAt(0).toUpperCase() : "U"}
          </div>
          <div>
            <Text size="sm" variant="default" className="font-bold">
              {username || t("preferences.defaultUsername")}
            </Text>
            <Text size="xs" variant="muted">
              {t("preferences.userRole")}
            </Text>
          </div>
        </Flex>
      </section>

      {/* Section 2: App Preferences */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.appSection")}
          </Heading>
        </div>

        {fetchingConfig ? (
          <div className="h-10 animate-pulse bg-muted rounded-xl" />
        ) : (
          <Grid cols={1} smCols={2} gap={4}>
            <div>
              <Text size="xs" variant="default" className="font-semibold mb-1.5 block">
                {t("preferences.themeLabel")}
              </Text>
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
              <Text size="xs" variant="default" className="font-semibold mb-1.5 block">
                {t("preferences.languageLabel")}
              </Text>
              <Select
                value={language}
                onChange={(val) => handleLanguageSelect(val as UserLanguage)}
                options={[
                  { value: UserLanguage.Polish, label: t("preferences.langPolish") },
                  { value: UserLanguage.English, label: t("preferences.langEnglish") },
                ]}
              />
            </div>
          </Grid>
        )}
      </section>

      {/* Section 3: Limit Dzienny */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.usageSection")}
          </Heading>
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
                const totalPct = Math.min(100, Math.max(0, usage.dailyUsagePercentage ?? 0));

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
                    <Flex justify="between" align="baseline">
                      <Text size="lg" variant="primary" className="font-bold">
                        {totalPct}%
                      </Text>
                    </Flex>

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
        <Flex align="center" justify="between" className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.cezSection")}
          </Heading>
          <Badge variant={isCezConnected ? "success" : "warning"} uppercase>
            {isCezConnected ? t("preferences.connected") : t("preferences.notConnected")}
          </Badge>
        </Flex>
        <div className="space-y-3.5">
          <Text size="sm" variant="muted">
            {t("courses.syncSubtitleDate", { date: formattedSyncDate })}
          </Text>
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

      {/* Section 5: Security & Session */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.securitySection")}
          </Heading>
        </div>
        <div className="space-y-3.5">
          <Text size="sm" variant="muted">
            {t("preferences.logoutDesc")}
          </Text>
          <div>
            <SecondaryButton
              onClick={handleLogout}
            >
              {t("common.logout")}
            </SecondaryButton>
          </div>
        </div>
      </section>
    </div>
  );
}
