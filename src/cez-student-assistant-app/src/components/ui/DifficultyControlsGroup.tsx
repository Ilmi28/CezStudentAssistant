import MultiSegmentProgressBar from "./MultiSegmentProgressBar";

interface DifficultyControlsGroupProps {
  easyCount: number;
  mediumCount: number;
  hardCount: number;
  setEasyCount: (val: number | ((prev: number) => number)) => void;
  setMediumCount: (val: number | ((prev: number) => number)) => void;
  setHardCount: (val: number | ((prev: number) => number)) => void;
  totalSelected: number;
  itemUnitLabel: string;
  maxPerCategory?: number;
}

export default function DifficultyControlsGroup({
  easyCount,
  mediumCount,
  hardCount,
  setEasyCount,
  setMediumCount,
  setHardCount,
  totalSelected,
  itemUnitLabel,
  maxPerCategory = 20,
}: DifficultyControlsGroupProps) {
  return (
    <div className="space-y-3.5 pt-2 border-t border-border/60">
      <div className="flex items-center justify-between text-xs font-semibold">
        <span className="text-foreground font-medium">Trudność</span>
        <span key={totalSelected} className="text-foreground font-bold text-xs bg-secondary px-2.5 py-0.5 rounded-full border border-border transition-all duration-300 animate-in fade-in duration-200">
          {totalSelected} {itemUnitLabel}
        </span>
      </div>

      <MultiSegmentProgressBar
        segments={[
          { id: "easy", value: easyCount, colorClass: "bg-emerald-500", customTooltip: `Łatwe (${easyCount})` },
          { id: "medium", value: mediumCount, colorClass: "bg-amber-500", customTooltip: `Średnie (${mediumCount})` },
          { id: "hard", value: hardCount, colorClass: "bg-rose-500", customTooltip: `Trudne (${hardCount})` },
        ]}
        heightClass="h-3.5"
      />

      <div className="space-y-2 pt-1">
        <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
          <div className="flex items-center gap-2">
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 shadow-sm shrink-0" />
            <span className="text-xs font-semibold text-foreground">Łatwe</span>
          </div>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              onClick={() => setEasyCount((prev) => Math.max(0, prev - 1))}
              disabled={easyCount <= 0}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              -
            </button>
            <span key={easyCount} className="w-6 text-center text-xs font-bold tabular-nums text-foreground animate-in fade-in zoom-in-95 duration-200">
              {easyCount}
            </span>
            <button
              type="button"
              onClick={() => setEasyCount((prev) => Math.min(maxPerCategory, prev + 1))}
              disabled={easyCount >= maxPerCategory}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              +
            </button>
          </div>
        </div>

        <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
          <div className="flex items-center gap-2">
            <span className="w-2.5 h-2.5 rounded-full bg-amber-500 shadow-sm shrink-0" />
            <span className="text-xs font-semibold text-foreground">Średnie</span>
          </div>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              onClick={() => setMediumCount((prev) => Math.max(0, prev - 1))}
              disabled={mediumCount <= 0}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              -
            </button>
            <span key={mediumCount} className="w-6 text-center text-xs font-bold tabular-nums text-foreground animate-in fade-in zoom-in-95 duration-200">
              {mediumCount}
            </span>
            <button
              type="button"
              onClick={() => setMediumCount((prev) => Math.min(maxPerCategory, prev + 1))}
              disabled={mediumCount >= maxPerCategory}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              +
            </button>
          </div>
        </div>

        <div className="p-2.5 px-3 rounded-xl bg-card border border-border flex items-center justify-between hover:border-border/80 transition-colors">
          <div className="flex items-center gap-2">
            <span className="w-2.5 h-2.5 rounded-full bg-rose-500 shadow-sm shrink-0" />
            <span className="text-xs font-semibold text-foreground">Trudne</span>
          </div>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              onClick={() => setHardCount((prev) => Math.max(0, prev - 1))}
              disabled={hardCount <= 0}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              -
            </button>
            <span key={hardCount} className="w-6 text-center text-xs font-bold tabular-nums text-foreground animate-in fade-in zoom-in-95 duration-200">
              {hardCount}
            </span>
            <button
              type="button"
              onClick={() => setHardCount((prev) => Math.min(maxPerCategory, prev + 1))}
              disabled={hardCount >= maxPerCategory}
              className="btn-app-spring w-7 h-7 rounded-lg bg-secondary border border-border flex items-center justify-center text-foreground hover:bg-secondary/80 disabled:opacity-30 disabled:transform-none cursor-pointer text-sm font-bold select-none"
            >
              +
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
