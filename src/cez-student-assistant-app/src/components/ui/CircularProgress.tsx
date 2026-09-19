import React from "react";

export interface CircularProgressProps {
  percentage: number;
  size?: number;
  strokeWidth?: number;
  className?: string;
  title?: string;
}

export const CircularProgress: React.FC<CircularProgressProps> = ({
  percentage,
  size = 20,
  strokeWidth = 2.5,
  className = "",
  title,
}) => {
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const clampedPercentage = Math.min(100, Math.max(0, percentage));
  const strokeDashoffset = circumference - (clampedPercentage / 100) * circumference;

  let colorClass = "text-primary";
  if (clampedPercentage >= 90) {
    colorClass = "text-rose-500";
  } else if (clampedPercentage >= 75) {
    colorClass = "text-amber-500";
  }

  return (
    <div
      className={`relative inline-flex items-center justify-center shrink-0 cursor-help ${className}`}
      title={title}
    >
      <svg
        width={size}
        height={size}
        viewBox={`0 0 ${size} ${size}`}
        className="transform -rotate-90"
      >
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke="currentColor"
          strokeWidth={strokeWidth}
          className="text-border/60 fill-none"
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke="currentColor"
          strokeWidth={strokeWidth}
          strokeDasharray={circumference}
          strokeDashoffset={strokeDashoffset}
          strokeLinecap="round"
          className={`${colorClass} fill-none transition-all duration-500 ease-out`}
        />
      </svg>
    </div>
  );
};

export default CircularProgress;
