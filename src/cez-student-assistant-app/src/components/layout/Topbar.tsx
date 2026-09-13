import Navbar from "./Navbar";
import UserControls from "./UserControls";

interface TopbarProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  fullName?: string | null;
  onLogout: () => void;
}

export default function Topbar({
  loading,
  onRefresh,
  username,
  fullName,
  onLogout,
}: TopbarProps) {
  return (
    <header className="bg-sidebar px-8 py-4 flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-sidebar-border sticky top-0 z-30">
      <Navbar />
      <UserControls
        loading={loading}
        onRefresh={onRefresh}
        username={username}
        fullName={fullName}
        onLogout={onLogout}
      />
    </header>
  );
}
