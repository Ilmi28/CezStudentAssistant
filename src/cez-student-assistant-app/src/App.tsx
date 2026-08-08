import { RefreshCw, CheckCircle, AlertCircle, X, BookOpen } from "lucide-react";
import { useTranslation } from "react-i18next";
import { AppProvider, useApp } from "./contexts/AppContext";
import AppRoutes from "./AppRoutes";

function AppShell() {
  const { isAuthChecking, errorMsg, successMsg, setErrorMsg, setSuccessMsg } = useApp();
  const { t } = useTranslation();

  if (isAuthChecking) {
    return (
      <div className="h-screen w-screen flex flex-col items-center justify-center bg-background text-foreground antialiased">
        <div className="flex flex-col items-center gap-4 animate-in fade-in duration-300">
          <div className="w-16 h-16 rounded bg-primary border-2 border-white/20 flex items-center justify-center shadow-lg animate-pulse">
            <BookOpen size={28} className="text-white" />
          </div>
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
      {errorMsg && (
        <div className="fixed bottom-4 right-4 z-50 bg-[#c44444] text-white px-5 py-3.5 rounded-lg shadow-xl flex items-center gap-3 border border-[#a23333] transition-all animate-bounce">
          <AlertCircle size={18} />
          <span className="text-[13px] font-medium">{errorMsg}</span>
          <button onClick={() => setErrorMsg(null)} className="ml-2 hover:opacity-80"><X size={15} /></button>
        </div>
      )}
      {successMsg && (
        <div className="fixed bottom-4 right-4 z-50 bg-[#006633] text-white px-5 py-3.5 rounded-lg shadow-xl flex items-center gap-3 border border-[#005229] transition-all">
          <CheckCircle size={18} />
          <span className="text-[13px] font-medium">{successMsg}</span>
          <button onClick={() => setSuccessMsg(null)} className="ml-2 hover:opacity-80"><X size={15} /></button>
        </div>
      )}
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
