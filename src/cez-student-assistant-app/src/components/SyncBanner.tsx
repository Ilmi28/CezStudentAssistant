import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import { PrimaryButton } from "./Button";

interface SyncBannerProps {
  syncing: boolean;
  onSyncCourses: () => void;
  isCezConnected?: boolean;
  lastCezSync?: string | null;
}

export default function SyncBanner({
  syncing,
  onSyncCourses,
  isCezConnected = false,
  lastCezSync,
}: SyncBannerProps) {
  const { t } = useTranslation();

  if (!isCezConnected) return null;

  const formattedDate = lastCezSync
    ? new Date(lastCezSync).toLocaleString("pl-PL", { dateStyle: "short", timeStyle: "short" })
    : null;

  return (
    <div className="bg-card rounded-xl border border-border p-5.5 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
      <div className="flex items-center gap-4">
        <div className="w-10 h-10 rounded-xl bg-muted flex items-center justify-center text-primary flex-shrink-0">
          <RefreshCw size={20} className={syncing ? "animate-spin" : ""} />
        </div>
        <div>
          <h3 className="text-sm font-semibold text-foreground">
            {t("courses.syncTitle")}
          </h3>
          {formattedDate && (
            <p className="text-[12px] text-muted-foreground mt-0.5">
              {t("courses.syncSubtitleDate", { date: formattedDate })}
            </p>
          )}
        </div>
      </div>
      <div className="flex gap-2.5">
        <PrimaryButton
          onClick={onSyncCourses}
          loading={syncing}
          icon={!syncing ? <RefreshCw size={14} /> : undefined}
        >
          {t("courses.syncBtn")}
        </PrimaryButton>
      </div>
    </div>
  );
}
