import React, { useState } from "react";
import { createPortal } from "react-dom";

export interface ProgressBarSegment {
  id: string;
  value: number;
  colorClass: string;
  tooltipLabel?: string;
  customTooltip?: string;
}

export interface MultiSegmentProgressBarProps {
  segments: ProgressBarSegment[];
  totalValue?: number;
  heightClass?: string;
  className?: string;
  showRemainingSegment?: boolean;
  remainingSegmentTooltip?: string;
}

export const MultiSegmentProgressBar: React.FC<MultiSegmentProgressBarProps> = ({
  segments,
  totalValue,
  heightClass = "h-3.5",
  className = "",
  showRemainingSegment = false,
  remainingSegmentTooltip,
}) => {
  const [tooltipInfo, setTooltipInfo] = useState<string | null>(null);
  const [tooltipPos, setTooltipPos] = useState<{ x: number; y: number }>({ x: 0, y: 0 });

  const calculatedTotal = totalValue ?? segments.reduce((sum, seg) => sum + seg.value, 0);

  const handleHover = (e: React.MouseEvent, infoText: string) => {
    setTooltipInfo(infoText);
    setTooltipPos({ x: e.clientX, y: e.clientY });
  };

  const sumSegments = segments.reduce((sum, seg) => sum + seg.value, 0);
  const remainingValue = Math.max(0, calculatedTotal - sumSegments);
  const remainingPct = calculatedTotal > 0 ? Math.max(0, (remainingValue / calculatedTotal) * 100) : 0;

  return (
    <div
      className={`relative w-full ${className}`}
      onMouseLeave={() => setTooltipInfo(null)}
    >
      <div
        className={`w-full bg-secondary ${heightClass} rounded-full flex gap-0.5 p-0.5 border border-border/50 overflow-hidden shadow-xs relative`}
      >
        {segments.map((seg) => {
          if (seg.value <= 0) return null;
          const pct = calculatedTotal > 0 ? Math.min(100, (seg.value / calculatedTotal) * 100) : 0;
          const pctStr = calculatedTotal > 0 ? ((seg.value / calculatedTotal) * 100).toFixed(1) : "0";
          const labelPrefix = seg.tooltipLabel ? `${seg.tooltipLabel}: ` : "";
          const text = seg.customTooltip || `${labelPrefix}${seg.value.toLocaleString()} (${pctStr}%)`;

          return (
            <div
              key={seg.id}
              style={{ width: `${pct}%` }}
              className={`h-full ${seg.colorClass} rounded-full cursor-pointer transition-all duration-500 ease-out`}
              onMouseMove={(e) => handleHover(e, text)}
              onMouseEnter={(e) => handleHover(e, text)}
            />
          );
        })}

        {showRemainingSegment && remainingPct > 0 && (
          <div
            style={{ width: `${remainingPct}%` }}
            className="h-full bg-transparent cursor-pointer transition-all duration-500 ease-out"
            onMouseMove={(e) => {
              const remPctStr = calculatedTotal > 0 ? ((remainingValue / calculatedTotal) * 100).toFixed(1) : "0";
              const text = remainingSegmentTooltip || `Wolny limit: ${remainingValue.toLocaleString()} (${remPctStr}%)`;
              handleHover(e, text);
            }}
            onMouseEnter={(e) => {
              const remPctStr = calculatedTotal > 0 ? ((remainingValue / calculatedTotal) * 100).toFixed(1) : "0";
              const text = remainingSegmentTooltip || `Wolny limit: ${remainingValue.toLocaleString()} (${remPctStr}%)`;
              handleHover(e, text);
            }}
          />
        )}
      </div>

      {tooltipInfo &&
        createPortal(
          <div
            style={{
              position: "fixed",
              left: tooltipPos.x,
              top: tooltipPos.y - 38,
              transform: "translateX(-50%)",
              pointerEvents: "none",
              zIndex: 99999,
            }}
            className="px-2.5 py-1 text-[11px] font-semibold bg-slate-950/95 text-white border border-border/80 shadow-2xl rounded-md whitespace-nowrap"
          >
            {tooltipInfo}
          </div>,
          document.body
        )}
    </div>
  );
};

export default MultiSegmentProgressBar;
