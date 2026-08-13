import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Home, Layers, Brain } from "lucide-react";
import pbEmblem from "../assets/pb-emblem.png";

export default function Navbar() {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const isHomeActive = pathname === "/home" || pathname === "/";
  const isCoursesActive = pathname === "/courses" || pathname.startsWith("/course/");
  const isQuizzesActive = pathname === "/quizzes" || pathname.startsWith("/quiz/");

  return (
    <div className="flex flex-col md:flex-row md:items-center gap-6 lg:gap-10">
      {/* Brand Logo & Name */}
      <div className="flex items-center gap-3.5 cursor-pointer group" onClick={() => navigate("/home")}>
        <img src={pbEmblem} alt="Politechnika Białostocka" className="h-11 w-auto object-contain flex-shrink-0 transition-transform group-hover:scale-105" />
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
        <button
          onClick={() => navigate("/home")}
          className={`flex items-center gap-2 px-3.5 py-2 rounded-xl text-sm font-medium transition-all cursor-pointer ${
            isHomeActive
              ? "bg-white/15 text-white font-semibold shadow-xs"
              : "text-white/70 hover:text-white hover:bg-white/10"
          }`}
        >
          <Home size={16} />
          <span>{t("nav.home")}</span>
        </button>
        <button
          onClick={() => navigate("/courses")}
          className={`flex items-center gap-2 px-3.5 py-2 rounded-xl text-sm font-medium transition-all cursor-pointer ${
            isCoursesActive
              ? "bg-white/15 text-white font-semibold shadow-xs"
              : "text-white/70 hover:text-white hover:bg-white/10"
          }`}
        >
          <Layers size={16} />
          <span>{t("nav.courses")}</span>
        </button>
        <button
          onClick={() => navigate("/quizzes")}
          className={`flex items-center gap-2 px-3.5 py-2 rounded-xl text-sm font-medium transition-all cursor-pointer ${
            isQuizzesActive
              ? "bg-white/15 text-white font-semibold shadow-xs"
              : "text-white/70 hover:text-white hover:bg-white/10"
          }`}
        >
          <Brain size={16} />
          <span>{t("nav.quizzes")}</span>
        </button>
      </nav>
    </div>
  );
}
