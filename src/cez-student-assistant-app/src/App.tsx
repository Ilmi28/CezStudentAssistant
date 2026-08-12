import { RefreshCw } from "lucide-react";
import { useTranslation } from "react-i18next";
import { AppProvider } from "./contexts";
import { useAuth, useUI } from "./hooks";
import AppRoutes from "./AppRoutes";
import Toast from "./components/Toast";
import pbLogo from "./assets/pb-logo.png";

function AppShell() {
  const { isAuthChecking } = useAuth();
  const { errorMsg, successMsg, setErrorMsg, setSuccessMsg } = useUI();
  const { t } = useTranslation();

  if (isAuthChecking) {
    return (
      <div className="h-screen w-screen flex flex-col items-center justify-center bg-background text-foreground antialiased">
        <div className="flex flex-col items-center gap-4 animate-in fade-in duration-300">
          <img src={pbLogo} alt="Politechnika Białostocka" className="w-16 h-16 object-contain animate-pulse" />
          <div className="flex items-center gap-2">
            <RefreshCw size={15} className="animate-spin text-primary" />
            <span className="text-[13px] font-medium text-muted-foreground font-mono">{t("common.loading")}</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen bg-background text-foreground flex flex-col overflow-hidden antialiased">
      <Toast variant="error" message={errorMsg} onClose={() => setErrorMsg(null)} />
      <Toast variant="success" message={successMsg} onClose={() => setSuccessMsg(null)} />
      <AppRoutes />
    </div>
  );
}

export default function App() {
  return (
    <AppProvider>
      <AppShell />
    </AppProvider>
  );
}
