import { useTranslation } from "react-i18next";
import Spinner from "./Spinner";

interface LoadingScreenProps {
  message?: string;
  fullScreen?: boolean;
  className?: string;
}

export default function LoadingScreen({
  message,
  fullScreen = false,
  className = "",
}: LoadingScreenProps) {
  const { t } = useTranslation();
  const displayMessage = message ?? t("common.loading");

  if (fullScreen) {
    return (
      <div
        className={`h-screen w-screen flex flex-col items-center justify-center bg-background text-foreground antialiased ${className}`}
      >
        <div className="flex flex-col items-center gap-4 animate-in fade-in duration-300">
          <div className="relative flex items-center justify-center">
            <div className="absolute w-20 h-20 rounded-full bg-primary/10 blur-xl animate-pulse" />
            <Spinner size="lg" />
          </div>
          {displayMessage && (
            <span className="text-xs font-medium tracking-wide uppercase text-muted-foreground animate-pulse">
              {displayMessage}
            </span>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className={`flex flex-col items-center justify-center py-16 px-4 ${className}`}>
      <div className="flex items-center gap-3.5 bg-card/80 border border-border px-6 py-4 rounded-xl shadow-lg shadow-black/20 animate-in fade-in duration-300">
        <Spinner size="sm" />
        {displayMessage && (
          <span className="text-xs font-medium tracking-wide text-muted-foreground">
            {displayMessage}
          </span>
        )}
      </div>
    </div>
  );
}
