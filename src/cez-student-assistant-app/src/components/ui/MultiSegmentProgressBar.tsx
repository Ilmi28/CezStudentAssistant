import React, { useState } from "react";
import { createPortal } from "react-dom";

export interface ProgressBarSegment {
  id: string;
  value: number;
  colorClass: string;
  tooltipLabel?: string;
  customTooltip?: string;
  animated?: boolean;
}

export interface MultiSegmentProgressBarProps {
  segments: ProgressBarSegment[];
  totalValue?: number;
  heightClass?: string;
  className?: string;
  showRemainingSegment?: boolean;
  remainingSegmentTooltip?: string;
}

interface ActiveHoverInfo {
  text: string;
  x: number;
  y: number;
}

export const MultiSegmentProgressBar: React.FC<MultiSegmentProgressBarProps> = ({
  segments,
  totalValue,
  heightClass = "h-3.5",
  className = "",
  showRemainingSegment = false,
  remainingSegmentTooltip,
}) => {
  const [activeHover, setActiveHover] = useState<ActiveHoverInfo | null>(null);

  const calculatedTotal = totalValue ?? segments.reduce((sum, seg) => sum + seg.value, 0);
  const sumSegments = segments.reduce((sum, seg) => sum + seg.value, 0);
  const remainingValue = Math.max(0, calculatedTotal - sumSegments);
  const remainingPct = calculatedTotal > 0 ? Math.max(0, (remainingValue / calculatedTotal) * 100) : 0;

  const handleMouseEnter = (e: React.MouseEvent<HTMLDivElement>, text: string) => {
    const rect = e.currentTarget.getBoundingClientRect();
    setActiveHover({
      text,
      x: rect.left + rect.width / 2,
      y: rect.top - 6,
    });
  };

  return (
    <div
      className={`relative w-full ${className}`}
      onMouseLeave={() => setActiveHover(null)}
    >
      <div className={`w-full bg-secondary ${heightClass} rounded-full flex gap-0.5 p-0.5 border border-border/50 overflow-hidden shadow-xs relative`}>
        {segments.map((seg) => {
          if (seg.value <= 0) return null;
          const pct = calculatedTotal > 0 ? Math.min(100, (seg.value / calculatedTotal) * 100) : 0;
          const pctStr = calculatedTotal > 0 ? ((seg.value / calculatedTotal) * 100).toFixed(1) : "0";
          const labelPrefix = seg.tooltipLabel ? `${seg.tooltipLabel}: ` : "";
          const text = seg.customTooltip || `${labelPrefix}${seg.value.toLocaleString()} (${pctStr}%)`;
          const isAnimated = seg.animated !== false;

          return (
            <div
              key={seg.id}
              style={{ width: `${pct}%` }}
              className={`h-full cursor-pointer ${isAnimated ? "transition-[width] duration-500 ease-out" : ""}`}
              onMouseEnter={(e) => handleMouseEnter(e, text)}
            >
              <div className={`w-full h-full ${seg.colorClass} rounded-full ${isAnimated ? "transition-all duration-500 ease-out" : ""} hover:brightness-110`} />
            </div>
          );
        })}

        {showRemainingSegment && remainingPct > 0 && (
          <div
            style={{ width: `${remainingPct}%` }}
            className="h-full cursor-pointer bg-transparent transition-[width] duration-500 ease-out"
            onMouseEnter={(e) => {
              const remPctStr = calculatedTotal > 0 ? ((remainingValue / calculatedTotal) * 100).toFixed(1) : "0";
              const text = remainingSegmentTooltip || `Wolny limit: ${remainingValue.toLocaleString()} (${remPctStr}%)`;
              handleMouseEnter(e, text);
            }}
          />
        )}
      </div>

      {activeHover &&
        createPortal(
          <div
            style={{
              position: "fixed",
              left: activeHover.x,
              top: activeHover.y,
              transform: "translate(-50%, -100%)",
              pointerEvents: "none",
              zIndex: 99999,
            }}
            className="flex flex-col items-center animate-in fade-in duration-150"
          >
            <div className="px-3 py-1.5 text-[11px] font-semibold bg-slate-900 border border-slate-700 text-white shadow-2xl rounded-lg whitespace-nowrap">
              {activeHover.text}
            </div>
            <div className="w-2 h-2 bg-slate-900 border-r border-b border-slate-700 rotate-45 -mt-1" />
          </div>,
          document.body
        )}
    </div>
  );
};

export default MultiSegmentProgressBar;
