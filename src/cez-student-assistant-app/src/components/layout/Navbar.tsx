import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import pbEmblem from "../../assets/pb-emblem.png";

interface NavItemProps {
  label: string;
  isActive: boolean;
  onClick: () => void;
}

function NavItem({ label, isActive, onClick }: NavItemProps) {
  return (
    <button
      onClick={onClick}
      className={`group btn-app-spring px-3.5 py-2 rounded-xl text-sm font-semibold cursor-pointer select-none transition-all ${
        isActive
          ? "bg-white/20 text-white border border-white/25 shadow-xs font-semibold"
          : "text-white/75 hover:text-white hover:bg-white/10 border border-transparent"
      }`}
    >
      <span>{label}</span>
    </button>
  );
}

export default function Navbar() {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const navItems = [
    {
      path: "/home",
      label: t("nav.home"),
      isActive: pathname === "/home" || pathname === "/",
    },
    {
      path: "/courses",
      label: t("nav.courses"),
      isActive: pathname === "/courses" || pathname.startsWith("/course/"),
    },
    {
      path: "/quizzes",
      label: t("nav.quizzes"),
      isActive: pathname === "/quizzes" || pathname.startsWith("/quiz/"),
    },
    {
      path: "/flashcards",
      label: t("nav.flashcards", "Fiszki"),
      isActive: pathname === "/flashcards" || pathname.startsWith("/flashcard"),
    },
    {
      path: "/chats",
      label: t("nav.chats", "Czaty"),
      isActive: pathname === "/chats" || pathname.startsWith("/chat"),
    },
  ];

  return (
    <div className="flex flex-col md:flex-row md:items-center gap-6 lg:gap-10">
      {/* Brand Logo & Name */}
      <div
        className="flex items-center gap-3.5 cursor-pointer group btn-app-spring select-none"
        onClick={() => navigate("/home")}
      >
        <img
          src={pbEmblem}
          alt="Politechnika Białostocka"
          className="h-11 w-auto object-contain flex-shrink-0 transition-transform group-hover:scale-105"
        />
        <div>
          <div className="text-white font-bold text-lg leading-tight tracking-tight">
            CEZStudentAssistant
          </div>
          <div className="text-[10px] tracking-[0.16em] uppercase text-white/50 mt-0.5 font-semibold">
            Politechnika Białostocka
          </div>
        </div>
      </div>

      {/* Horizontal Navigation Links */}
      <nav className="flex items-center gap-2 lg:gap-3 border-t md:border-t-0 border-white/10 pt-3 md:pt-0">
        {navItems.map((item) => (
          <NavItem
            key={item.path}
            label={item.label}
            isActive={item.isActive}
            onClick={() => navigate(item.path)}
          />
        ))}
      </nav>
    </div>
  );
}
