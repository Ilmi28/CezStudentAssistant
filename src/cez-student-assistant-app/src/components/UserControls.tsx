import { useTranslation } from "react-i18next";
import { RefreshCw, Wifi } from "lucide-react";
import ProfileDropdown from "./ProfileDropdown";

interface UserControlsProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  onLogout: () => void;
}

export default function UserControls({
  loading,
  onRefresh,
  username,
  onLogout,
}: UserControlsProps) {
  const { t } = useTranslation();

  return (
    <div className="flex flex-wrap items-center justify-end gap-3 self-end md:self-auto">
      {/* Status indicator */}
      <div className="flex items-center gap-1.5 text-[11px] text-primary font-mono font-medium">
        <Wifi size={12} className="animate-pulse" /> {t("common.statusOnline")}
      </div>

      {/* Refresh button */}
      <button
        onClick={onRefresh}
        disabled={loading}
        className="p-1.5 rounded bg-white/5 text-white/60 hover:text-white hover:bg-white/10 transition-all border border-white/10 disabled:opacity-50 cursor-pointer"
        title={t("common.refreshSuccess")}
      >
        <RefreshCw size={14} className={loading ? "animate-spin" : ""} />
      </button>

      {/* Logged user profile & popup menu */}
      <ProfileDropdown username={username} onLogout={onLogout} />
    </div>
  );
}
