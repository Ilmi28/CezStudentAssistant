import { AppProvider } from "./contexts";
import { useAuth, useUI } from "./hooks";
import AppRoutes from "./AppRoutes";
import { Toast, AppSplashLoader } from "./components";

function AppShell() {
  const { isAuthChecking } = useAuth();
  const { errorMsg, successMsg, setErrorMsg, setSuccessMsg } = useUI();

  if (isAuthChecking) {
    return <AppSplashLoader />;
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
