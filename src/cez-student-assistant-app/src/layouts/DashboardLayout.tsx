import { Outlet } from "react-router-dom";
import { useApp } from "../contexts/AppContext";
import Topbar from "../components/Topbar";
import CezModal from "../components/CezModal";

export default function DashboardLayout() {
  const {
    loading,
    handleRefreshLists,
    username,
    handleLogout,
    darkMode,
    toggleDarkMode,
    showCezModal,
    setShowCezModal,
    handleCezLinkSubmit,
  } = useApp();

  return (
    <>
      <Topbar
        loading={loading}
        onRefresh={handleRefreshLists}
        username={username}
        onLogout={handleLogout}
        darkMode={darkMode}
        onToggleDarkMode={toggleDarkMode}
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
