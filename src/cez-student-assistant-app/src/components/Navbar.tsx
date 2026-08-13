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
    <div className="flex flex-col md:flex-row md:items-center gap-6 lg:gap-8">
      {/* Brand Logo & Name */}
      <div className="flex items-center gap-3 cursor-pointer" onClick={() => navigate("/home")}>
        <img src={pbEmblem} alt="Politechnika Białostocka" className="h-10 w-auto object-contain flex-shrink-0" />
        <div>
          <div className="text-white font-semibold text-base leading-tight">
            CEZStudentAssistant
          </div>
          <div className="text-[9px] tracking-[0.15em] uppercase text-white/40 mt-0.5 font-medium">
            Politechnika Białostocka
          </div>
        </div>
      </div>

      {/* Horizontal Navigation Links */}
      <nav className="flex items-center gap-5 lg:gap-6 border-t md:border-t-0 border-white/5 pt-2 md:pt-0">
        <button
          onClick={() => navigate("/home")}
          className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
            isHomeActive
              ? "border-white text-white"
              : "border-transparent text-white/60 hover:text-white/95"
          }`}
        >
          <Home size={13} />
          <span>{t("nav.home")}</span>
        </button>
        <button
          onClick={() => navigate("/courses")}
          className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
            isCoursesActive
              ? "border-white text-white"
              : "border-transparent text-white/60 hover:text-white/95"
          }`}
        >
          <Layers size={13} />
          <span>{t("nav.courses")}</span>
        </button>
        <button
          onClick={() => navigate("/quizzes")}
          className={`flex items-center gap-1.5 py-1.5 text-[13px] font-semibold transition-all border-b-2 cursor-pointer ${
            isQuizzesActive
              ? "border-white text-white"
              : "border-transparent text-white/60 hover:text-white/95"
          }`}
        >
          <Brain size={13} />
          <span>{t("nav.quizzes")}</span>
        </button>
      </nav>
    </div>
  );
}
