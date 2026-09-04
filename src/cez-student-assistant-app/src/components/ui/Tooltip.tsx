import React, { useState } from "react";
import { createPortal } from "react-dom";

interface TooltipProps {
  content: string;
  children: React.ReactNode;
  className?: string;
}

export default function Tooltip({ content, children, className = "" }: TooltipProps) {
  const [coords, setCoords] = useState<{ x: number; y: number } | null>(null);

  const handleMouseEnter = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    setCoords({
      x: rect.left + rect.width / 2,
      y: rect.top - 8,
    });
  };

  const handleMouseLeave = () => {
    setCoords(null);
  };

  return (
    <div
      className={`inline-flex items-center ${className}`}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {children}
      {coords &&
        createPortal(
          <div
            style={{
              position: "fixed",
              left: coords.x,
              top: coords.y,
              transform: "translate(-50%, -100%)",
              pointerEvents: "none",
              zIndex: 99999,
            }}
            className="flex flex-col items-center animate-in fade-in duration-150"
          >
            <div className="px-3 py-1.5 text-[11px] font-semibold bg-slate-900 border border-slate-700/80 text-slate-100 shadow-2xl rounded-lg whitespace-nowrap">
              {content}
            </div>
            <div className="w-2 h-2 bg-slate-900 border-r border-b border-slate-700/80 rotate-45 -mt-1" />
          </div>,
          document.body
        )}
    </div>
  );
}
