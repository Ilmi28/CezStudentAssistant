import { useTranslation } from "react-i18next";
import pbEmblem from "../assets/pb-emblem.png";

interface AppSplashLoaderProps {
  message?: string;
}

export default function AppSplashLoader({ message }: AppSplashLoaderProps) {
  const { t } = useTranslation();
  const displayMessage = message ?? t("common.loading");

  return (
    <div className="h-screen w-screen flex flex-col items-center justify-center bg-background text-foreground antialiased selection:bg-primary/30 relative overflow-hidden">
      {/* Soft ambient background glow */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-80 h-80 bg-primary/10 rounded-full blur-3xl pointer-events-none" />

      <div className="relative flex flex-col items-center gap-5 animate-in fade-in zoom-in-95 duration-500 max-w-xs text-center px-4">
        {/* Emblem with glow */}
        <div className="relative flex items-center justify-center">
          <div className="absolute -inset-1.5 bg-gradient-to-r from-primary/30 to-sky-500/30 rounded-2xl blur-md animate-pulse" />
          <div className="relative w-16 h-16 rounded-2xl bg-card border border-border/90 flex items-center justify-center shadow-xl">
            <img
              src={pbEmblem}
              alt="Politechnika Białostocka"
              className="h-10 w-auto object-contain"
            />
          </div>
        </div>

        {/* Title and subtitle */}
        <div className="space-y-0.5">
          <h1 className="text-lg font-bold text-foreground tracking-tight">
            CEZStudentAssistant
          </h1>
          <p className="text-[10px] tracking-[0.2em] uppercase text-muted-foreground font-semibold">
            Politechnika Białostocka
          </p>
        </div>

        {/* Animated slender loading beam */}
        <div className="w-40 space-y-2 pt-1">
          <div className="w-full h-1 bg-secondary rounded-full overflow-hidden relative">
            <div className="absolute top-0 bottom-0 left-0 w-1/2 bg-gradient-to-r from-transparent via-primary to-sky-400 rounded-full animate-loading-beam" />
          </div>
          <span className="block text-[11px] font-medium tracking-wide text-muted-foreground/75 animate-pulse">
            {displayMessage}
          </span>
        </div>
      </div>
    </div>
  );
}
