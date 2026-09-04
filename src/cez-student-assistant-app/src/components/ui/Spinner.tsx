interface SpinnerProps {
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
}

const sizeClasses = {
  sm: "w-4 h-4",
  md: "w-7 h-7",
  lg: "w-10 h-10",
  xl: "w-14 h-14",
};

export default function Spinner({ size = "md", className = "" }: SpinnerProps) {
  return (
    <div className={`relative flex items-center justify-center ${sizeClasses[size]} ${className}`}>
      <svg
        className="w-full h-full animate-spin text-primary"
        viewBox="0 0 36 36"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <circle
          cx="18"
          cy="18"
          r="14"
          stroke="currentColor"
          strokeWidth="3.5"
          className="opacity-15"
        />
        <circle
          cx="18"
          cy="18"
          r="14"
          stroke="currentColor"
          strokeWidth="3.5"
          strokeDasharray="65"
          strokeDashoffset="45"
          strokeLinecap="round"
          className="opacity-90 drop-shadow-[0_0_8px_rgba(37,99,235,0.5)]"
        />
      </svg>
    </div>
  );
}
