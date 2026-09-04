import { useTranslation } from "react-i18next";
import pbEmblem from "../../assets/pb-emblem.png";
import Spinner from "../ui/Spinner";

interface AppSplashLoaderProps {
  message?: string;
}

export default function AppSplashLoader({ message }: AppSplashLoaderProps) {
  const { t } = useTranslation();
  const displayMessage = message ?? t("common.loading");

  return (
    <div className="h-screen w-screen flex flex-col items-center justify-center bg-background text-foreground antialiased selection:bg-primary/30 relative">
      <div className="flex flex-col items-center gap-4 animate-in fade-in duration-300 max-w-xs text-center px-4">
        <img
          src={pbEmblem}
          alt="Politechnika Białostocka"
          className="h-12 w-auto object-contain mb-1"
        />

        <div className="space-y-0.5">
          <h1 className="text-base font-bold text-foreground tracking-tight">
            CEZ Student Assistant
          </h1>
          <p className="text-[10px] tracking-[0.16em] uppercase text-muted-foreground font-semibold">
            Politechnika Białostocka
          </p>
        </div>

        <div className="flex items-center gap-2.5 pt-3">
          <Spinner size="sm" />
          <span className="text-xs font-medium text-muted-foreground">
            {displayMessage}
          </span>
        </div>
      </div>
    </div>
  );
}

