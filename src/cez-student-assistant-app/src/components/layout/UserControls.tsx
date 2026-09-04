import ProfileDropdown from "./ProfileDropdown";

interface UserControlsProps {
  loading?: boolean;
  onRefresh?: () => void;
  username: string | null;
  onLogout: () => void;
}

export default function UserControls({
  username,
  onLogout,
}: UserControlsProps) {
  return (
    <div className="flex items-center justify-end self-end md:self-auto">
      {/* Logged user profile & popup menu */}
      <ProfileDropdown username={username} onLogout={onLogout} />
    </div>
  );
}
