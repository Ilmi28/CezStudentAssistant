import React, { useEffect } from "react";
import { useTranslation } from "react-i18next";
import pbLogo from "../assets/pb-logo.png";

export interface AuthLayoutProps {
  children: React.ReactNode;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({ children }) => {
  const { i18n } = useTranslation();

  useEffect(() => {
    const savedLang = localStorage.getItem("language") || "pl";
    if (i18n.language !== savedLang) {
      i18n.changeLanguage(savedLang);
    }
    document.documentElement.classList.remove("dark");
  }, []);

  return (
    <div className="flex-1 flex items-center justify-center p-6 sm:p-12 bg-background relative">
      <div className="w-full max-w-md bg-card rounded-xl border border-border shadow-md overflow-hidden animate-in fade-in slide-in-from-bottom-2 duration-300 ease-out">
        {/* Academic Header */}
        <div className="bg-sidebar px-8 py-8 text-center border-b border-sidebar-border">
          <div className="flex justify-center mb-4">
            <img src={pbLogo} alt="Politechnika Białostocka" className="w-24 h-24 sm:w-28 sm:h-28 object-contain" />
          </div>
          <h1 className="text-white text-xl font-semibold leading-tight">
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
