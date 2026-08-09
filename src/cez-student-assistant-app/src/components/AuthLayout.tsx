import React from "react";
import { useTranslation } from "react-i18next";
import { Globe } from "lucide-react";
import pbLogo from "../assets/pb-logo.png";

export interface AuthLayoutProps {
  children: React.ReactNode;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({ children }) => {
  const { i18n } = useTranslation();

  const toggleLanguage = () => {
    const next = i18n.language === "pl" ? "en" : "pl";
    i18n.changeLanguage(next);
    localStorage.setItem("language", next);
  };

  return (
    <div className="flex-1 flex items-center justify-center p-6 sm:p-12 bg-background relative">
      {/* Floating Language Switcher */}
      <div className="absolute top-4 right-4 sm:top-6 sm:right-6 z-20">
        <button
          type="button"
          onClick={toggleLanguage}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-card border border-border text-foreground hover:bg-muted text-[12px] font-semibold transition-all shadow-xs cursor-pointer"
        >
          <Globe size={14} className="text-primary" />
          <span className="font-mono uppercase">{i18n.language === "pl" ? "PL" : "EN"}</span>
        </button>
      </div>

      <div className="w-full max-w-md bg-card rounded-xl border border-border shadow-md overflow-hidden animate-in fade-in duration-300">
        {/* Academic Header */}
        <div className="bg-sidebar px-8 py-8 text-center border-b border-sidebar-border">
          <div className="flex justify-center mb-4">
            <img src={pbLogo} alt="Politechnika Białostocka" className="w-24 h-24 sm:w-28 sm:h-28 object-contain" />
          </div>
          <h1 style={{ fontFamily: "Roboto Slab, serif" }} className="text-white text-xl font-semibold leading-tight">
            CEZ Student Assistant
          </h1>
          <p className="text-[10px] tracking-[0.15em] uppercase text-white/55 mt-1 font-medium">
            Politechnika Białostocka
          </p>
        </div>

        {/* Form Body */}
        <div className="p-8 space-y-5">
          {children}
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
