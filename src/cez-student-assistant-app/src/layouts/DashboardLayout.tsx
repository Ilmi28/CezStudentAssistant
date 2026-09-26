import { Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useQuiz } from "../hooks";
import { Topbar, CezModal } from "../components";
import { cezService, UnauthorizedError } from "../services";

export default function DashboardLayout() {
  const { username, fullName, handleLogout, handleCezLinkSubmit, setIsCezConnected, setLastCezSync } = useAuth();
  const { loading, setLoading, showCezModal, setShowCezModal, setError } = useUI();
  const { refreshCourses } = useCourse();
  const { refreshQuizzes } = useQuiz();
  const { t } = useTranslation();

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
        fullName={fullName}
        onLogout={handleLogout}
      />
      <main className="flex-1 w-full max-w-7xl mx-auto p-4 sm:p-6 pb-6 sm:pb-8 flex flex-col">
        <div className="flex-1 w-full flex flex-col">
          <Outlet />
        </div>
        {/* Compact bottom breathing room */}
        <div className="h-6 sm:h-8 shrink-0 w-full pointer-events-none" aria-hidden="true" />
      </main>
      <CezModal
        isOpen={showCezModal}
        onClose={() => setShowCezModal(false)}
        onSubmit={handleCezLinkSubmit}
        loading={loading}
      />
    </>
  );
}
