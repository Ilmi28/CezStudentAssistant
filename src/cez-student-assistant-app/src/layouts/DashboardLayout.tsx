import { Outlet, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, useUI, useCourse, useQuiz } from "../hooks";
import Topbar from "../components/Topbar";
import CezModal from "../components/CezModal";
import { cezService, UnauthorizedError } from "../services";

export default function DashboardLayout() {
  const location = useLocation();
  const { username, handleLogout, handleCezLinkSubmit, setIsCezConnected, setLastCezSync } = useAuth();
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
        onLogout={handleLogout}
      />
      <div className="flex-1 overflow-y-auto p-6 md:p-8">
        <div className="max-w-5xl mx-auto w-full space-y-8">
          <div key={location.pathname} className="animate-in fade-in slide-in-from-bottom-2 duration-300 ease-out">
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
