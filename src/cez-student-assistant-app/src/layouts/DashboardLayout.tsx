import { Outlet, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useQuiz } from "../hooks";
import { Topbar, CezModal } from "../components";
import { cezService, UnauthorizedError } from "../services";

export default function DashboardLayout() {
  const location = useLocation();
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
      <div className="flex-1 min-h-0 overflow-y-auto p-4 md:p-6 flex flex-col">
        <div className="max-w-7xl mx-auto w-full flex-1 min-h-0 flex flex-col pb-12 md:pb-16">
          <div key={location.pathname} className="flex-1 min-h-0 flex flex-col animate-in fade-in slide-in-from-bottom-2 duration-300 ease-out">
            <Outlet />
          </div>
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
