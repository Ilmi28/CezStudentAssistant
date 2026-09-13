import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Pencil } from "lucide-react";
import { useAuth, useUI, useCourse, useUser } from "../hooks";
import {
  PrimaryButton,
  SecondaryButton,
  Select,
  MultiSegmentProgressBar,
  Heading,
  Text,
  Flex,
  Grid,
  Input,
  SetPasswordModal,
  ChangePasswordModal,
  ConfirmModal,
  Alert,
} from "../components";
import { authService, userService } from "../services";
import { UserTheme, UserLanguage, type UserProfileDto } from "../types";

export default function PreferencesPage() {
  const [theme, setThemeState] = useState<UserTheme>(UserTheme.Dark);
  const [language, setLanguageState] = useState<UserLanguage>(UserLanguage.Polish);
  const [hasPassword, setHasPassword] = useState<boolean>(false);
  const [fetchingConfig, setFetchingConfig] = useState(true);
  const [userProfile, setUserProfile] = useState<UserProfileDto | null>(null);

  const [showSetPasswordModal, setShowSetPasswordModal] = useState(false);
  const [showChangePasswordModal, setShowChangePasswordModal] = useState(false);
  const [showDisconnectCezModal, setShowDisconnectCezModal] = useState(false);
  const [cezError, setCezError] = useState<string | null>(null);

  const [profileUserName, setProfileUserName] = useState("");
  const [profileFullName, setProfileFullName] = useState("");
  const [profileEmail, setProfileEmail] = useState("");
  const [savingProfile, setSavingProfile] = useState(false);
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);

  const {
    username,
    isCezConnected,
    lastCezSync,
    handleLogout,
    handleCezDisconnect,
    setIsCezConnected,
    setLastCezSync,
    checkAuthStatus
  } = useAuth();

  const { applyTheme, updateTheme, updateUserLanguage, setShowCezModal, syncing, setSuccess } = useUI();
  const { handleSyncCourses } = useCourse();
  const { usage, loadingUsage } = useUser();
  const { t, i18n } = useTranslation();

  useEffect(() => {
    async function loadConfig() {
      try {
        const [config, profile] = await Promise.all([
          userService.getUserConfiguration(),
          userService.getUserProfile().catch(() => null)
        ]);
        setThemeState(config.theme);
        applyTheme(config.theme);
        setLanguageState(config.language);
        setIsCezConnected(config.isCezConnected);
        setHasPassword(!!config.hasPassword);
        if (profile) setUserProfile(profile);
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

  useEffect(() => {
    if (userProfile) {
      setProfileUserName(userProfile.userName || "");
      setProfileFullName(userProfile.fullName || "");
      setProfileEmail(userProfile.email || "");
    } else if (username) {
      setProfileUserName(username);
    }
  }, [userProfile, username]);

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

  const handleSetPassword = async (newPassword: string) => {
    await authService.setPassword(newPassword);
    setHasPassword(true);
    setCezError(null);
    setSuccess(t("preferences.passwordSetSuccess"));
  };

  const handleChangePassword = async (currentPassword: string, newPassword: string) => {
    await authService.changePassword(currentPassword, newPassword);
    setSuccess(t("preferences.passwordChangeSuccess"));
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profileUserName.trim()) {
      setProfileError(t("editProfileModal.emptyUsernameError", "Nazwa użytkownika jest wymagana."));
      return;
    }

    setProfileError(null);
    setSavingProfile(true);
    try {
      const updated = await userService.updateUserProfile({
        userName: profileUserName.trim(),
        fullName: profileFullName.trim(),
        email: profileEmail.trim(),
      });
      setUserProfile(updated);
      setProfileUserName(updated.userName);
      setProfileFullName(updated.fullName || "");
      setProfileEmail(updated.email || "");
      if (profileUserName.trim() !== username) {
        localStorage.setItem("username", profileUserName.trim());
        await checkAuthStatus();
      }
      setSuccess(t("editProfileModal.successMessage", "Profil został pomyślnie zaktualizowany."));
      setIsEditingProfile(false);
    } catch (err: any) {
      setProfileError(err.message || t("common.genericError"));
    } finally {
      setSavingProfile(false);
    }
  };

  const handleDisconnectCezConfirm = async () => {
    if (!hasPassword) {
      setCezError(t("preferences.disconnectPureCezWarning"));
      return;
    }
    setCezError(null);
    await handleCezDisconnect();
    setSuccess(t("preferences.cezDisconnectSuccess"));
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
        <div className="flex items-center justify-between border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.profileSection")}
          </Heading>
          {!isEditingProfile && (
            <SecondaryButton
              type="button"
              size="sm"
              icon={<Pencil size={14} />}
              onClick={() => setIsEditingProfile(true)}
            >
              {t("common.edit", "Edytuj")}
            </SecondaryButton>
          )}
        </div>

        <form onSubmit={handleSaveProfile} className="space-y-4 py-1">
          <Alert message={profileError} />

          <Input
            label={t("editProfileModal.username", "Nazwa użytkownika")}
            value={profileUserName}
            readOnly={!isEditingProfile}
            onChange={(e) => {
              setProfileUserName(e.target.value);
              if (profileError) setProfileError(null);
            }}
          />

          <Input
            label={t("editProfileModal.fullName", "Imię i nazwisko")}
            value={profileFullName}
            readOnly={!isEditingProfile}
            onChange={(e) => {
              setProfileFullName(e.target.value);
              if (profileError) setProfileError(null);
            }}
          />

          <Input
            type="email"
            label={t("editProfileModal.email", "Adres e-mail")}
            value={profileEmail}
            readOnly={!isEditingProfile}
            onChange={(e) => {
              setProfileEmail(e.target.value);
              if (profileError) setProfileError(null);
            }}
          />

          {isEditingProfile && (
            <Flex justify="end" gap={3} className="pt-2 animate-in fade-in duration-150">
              <SecondaryButton
                type="button"
                onClick={() => {
                  setIsEditingProfile(false);
                  setProfileError(null);
                  if (userProfile) {
                    setProfileUserName(userProfile.userName || "");
                    setProfileFullName(userProfile.fullName || "");
                    setProfileEmail(userProfile.email || "");
                  } else if (username) {
                    setProfileUserName(username);
                  }
                }}
              >
                {t("common.cancel", "Anuluj")}
              </SecondaryButton>
              <PrimaryButton type="submit" loading={savingProfile}>
                {t("common.save", "Zapisz")}
              </PrimaryButton>
            </Flex>
          )}
        </form>
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
                  { value: UserTheme.System, label: t("preferences.themeSystem") },
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

      {/* Section 3: Daily Limit */}
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
          <span className={`text-xs font-bold uppercase tracking-wider ${isCezConnected ? "text-emerald-400" : "text-amber-400"}`}>
            {isCezConnected ? t("preferences.connected") : t("preferences.notConnected")}
          </span>
        </Flex>

        <Alert message={cezError} />

        <div className="space-y-3.5">
          <Text size="sm" variant="muted">
            {t("courses.syncSubtitleDate", { date: formattedSyncDate })}
          </Text>
          <Flex gap={3}>
            <SecondaryButton
              onClick={() => (isCezConnected ? handleSyncCourses() : setShowCezModal(true))}
              loading={syncing}
            >
              {isCezConnected ? t("preferences.syncNow") : t("courses.connectCezBtn")}
            </SecondaryButton>

            {isCezConnected && (
              <SecondaryButton
                onClick={() => {
                  setCezError(null);
                  if (!hasPassword) {
                    setCezError(t("preferences.disconnectPureCezWarning"));
                  } else {
                    setShowDisconnectCezModal(true);
                  }
                }}
              >
                {t("preferences.disconnectCez")}
              </SecondaryButton>
            )}
          </Flex>
        </div>
      </section>

      {/* Section 5: Security & Session */}
      <section className="space-y-4">
        <div className="border-b border-border pb-2.5">
          <Heading level={2} size="base" className="font-bold tracking-wide">
            {t("preferences.securitySection")}
          </Heading>
        </div>

        <div className="space-y-4">
          <div className="space-y-2">
            <Text size="xs" variant="default" className="font-semibold block">
              {t("preferences.passwordSection")}
            </Text>
            <div>
              <SecondaryButton
                onClick={() => {
                  if (hasPassword) {
                    setShowChangePasswordModal(true);
                  } else {
                    setShowSetPasswordModal(true);
                  }
                }}
              >
                {hasPassword ? t("preferences.changePasswordBtn") : t("preferences.setPasswordBtn")}
              </SecondaryButton>
            </div>
          </div>

          <div className="space-y-2">
            <Text size="xs" variant="default" className="font-semibold block">
              {t("preferences.logoutDesc")}
            </Text>
            <div>
              <SecondaryButton onClick={handleLogout}>
                {t("common.logout")}
              </SecondaryButton>
            </div>
          </div>
        </div>
      </section>

      {/* Modals */}
      <SetPasswordModal
        isOpen={showSetPasswordModal}
        onClose={() => setShowSetPasswordModal(false)}
        onSubmit={handleSetPassword}
      />

      <ChangePasswordModal
        isOpen={showChangePasswordModal}
        onClose={() => setShowChangePasswordModal(false)}
        onSubmit={handleChangePassword}
      />

      <ConfirmModal
        isOpen={showDisconnectCezModal}
        onClose={() => setShowDisconnectCezModal(false)}
        onConfirm={handleDisconnectCezConfirm}
        title={t("preferences.disconnectConfirmTitle")}
        message={t("preferences.disconnectConfirmMsg")}
        confirmBtnText={t("preferences.disconnectCez")}
        isDestructive={true}
      />
    </div>
  );
}
