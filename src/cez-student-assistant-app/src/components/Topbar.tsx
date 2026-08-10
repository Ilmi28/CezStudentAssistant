import Navbar from "./Navbar";
import UserControls from "./UserControls";

interface TopbarProps {
  loading: boolean;
  onRefresh: () => void;
  username: string | null;
  onLogout: () => void;
  darkMode: boolean;
  onToggleDarkMode: () => void;
}

export default function Topbar({
  loading,
  onRefresh,
  username,
  onLogout,
  darkMode,
  onToggleDarkMode,
}: TopbarProps) {
  return (
    <header className="bg-sidebar px-8 py-4 flex flex-col md:flex-row md:items-center md:justify-between gap-4 border-b border-sidebar-border sticky top-0 z-30">
      <Navbar />
      <UserControls
        loading={loading}
        onRefresh={onRefresh}
        username={username}
        onLogout={onLogout}
        darkMode={darkMode}
        onToggleDarkMode={onToggleDarkMode}
      />
    </header>
  );
}
