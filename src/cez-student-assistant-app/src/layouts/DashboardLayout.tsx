import { useEffect } from "react";
import { Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useQuiz } from "../hooks";
import Topbar from "../components/Topbar";
import CezModal from "../components/CezModal";
import { userService, cezService, UnauthorizedError } from "../services";
import { UserLanguage } from "../types";

export default function DashboardLayout() {
  const { username, handleLogout, handleCezLinkSubmit, setIsCezConnected, setLastCezSync } = useAuth();
  const { loading, setLoading, applyTheme, showCezModal, setShowCezModal, setError } = useUI();
  const { refreshCourses } = useCourse();
  const { refreshQuizzes } = useQuiz();
  const { t, i18n } = useTranslation();

  useEffect(() => {
    async function loadUserConfiguration() {
      try {
        const config = await userService.getUserConfiguration();
        applyTheme(config.theme);
        const lang = config.language === UserLanguage.English ? "en" : "pl";
        i18n.changeLanguage(lang);
        localStorage.setItem("language", lang);
      } catch (err) {
        console.debug("[DashboardLayout] Configuration load fallback:", err);
      }
    }
    loadUserConfiguration();
  }, []);

  const handleRefreshLists = async () => {
    setLoading(true);
    try {
      await Promise.all([refreshCourses(), refreshQuizzes()]);
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch (cezErr) {
        console.debug("[DashboardLayout] CEZ status fallback on refresh:", cezErr);
      }
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        return;
      }
      setError(t("common.errorConnection"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Topbar
        loading={loading}
        onRefresh={handleRefreshLists}
        username={username}
        onLogout={handleLogout}
      />
      <div className="flex-1 overflow-y-auto p-6 md:p-8">
        <div className="max-w-5xl mx-auto w-full space-y-8">
          <Outlet />
        </div>
      </div>
      <CezModal
        isOpen={showCezModal}
        onClose={() => setShowCezModal(false)}
        onSubmit={handleCezLinkSubmit}
        loading={loading}
      />
    </>
  );
}
