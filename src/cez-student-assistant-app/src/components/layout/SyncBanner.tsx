import { useTranslation } from "react-i18next";
import { RefreshCw } from "lucide-react";
import { Card, Heading, Text, Flex, PrimaryButton } from "../index";
import { formatDateTime } from "../../helpers/dateHelper";

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
    ? formatDateTime(lastCezSync, { dateStyle: "short", timeStyle: "short" })
    : null;

  return (
    <Card className="p-5.5 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
      <Flex align="center" gap={4}>
        <div className="w-10 h-10 rounded-xl bg-muted flex items-center justify-center text-primary shrink-0">
          <RefreshCw size={20} className={syncing ? "animate-spin" : ""} />
        </div>
        <div>
          <Heading level={3} size="sm" className="font-semibold">
            {t("courses.syncTitle")}
          </Heading>
          {formattedDate && (
            <Text size="xs" variant="muted" className="mt-0.5">
              {t("courses.syncSubtitleDate", { date: formattedDate })}
            </Text>
          )}
        </div>
      </Flex>
      <Flex gap={2.5}>
        <PrimaryButton
          onClick={onSyncCourses}
          loading={syncing}
          icon={!syncing ? <RefreshCw size={14} /> : undefined}
        >
          {t("courses.syncBtn")}
        </PrimaryButton>
      </Flex>
    </Card>
  );
}
