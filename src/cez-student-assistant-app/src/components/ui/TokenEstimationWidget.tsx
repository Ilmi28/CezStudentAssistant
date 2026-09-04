import MultiSegmentProgressBar from "./MultiSegmentProgressBar";

export interface TokenEstimationData {
  estimatedTokens: number;
  dailyTokenLimit: number;
  dailyTokensUsed: number;
  dailyTokensReserved?: number;
  estimatedDailyUsagePercentage: number;
  canGenerate: boolean;
}

interface TokenEstimationWidgetProps {
  hasFiles: boolean;
  loading: boolean;
  estimation: TokenEstimationData | null;
  itemLabel?: string;
}

export default function TokenEstimationWidget({
  hasFiles,
  loading,
  estimation,
  itemLabel = "Ten element",
}: TokenEstimationWidgetProps) {
  if (!hasFiles) return null;

  return (
    <div className="space-y-2 pt-1">
      <span className="block text-xs font-semibold text-foreground tracking-wide">
        Szacowanie zużycia
      </span>

      {loading ? (
        <div className="h-6 animate-pulse bg-muted rounded-lg" />
      ) : estimation ? (
        <div className="space-y-1.5">
          <div className="flex justify-between items-baseline">
            <span
              className={`text-sm font-bold ${
                estimation.canGenerate ? "text-foreground" : "text-destructive"
              }`}
            >
              ~{estimation.estimatedDailyUsagePercentage}%
            </span>
          </div>

          {(() => {
            const limit = estimation.dailyTokenLimit || 1;
            const realUsed = estimation.dailyTokensUsed || 0;
            const otherReserved = estimation.dailyTokensReserved || 0;
            const thisEstimated = estimation.estimatedTokens || 0;
            const remainingTokens = Math.max(0, limit - realUsed - otherReserved - thisEstimated);

            const realPctStr = limit > 0 ? ((realUsed / limit) * 100).toFixed(1) : "0";
            const otherReservedPctStr = limit > 0 ? ((otherReserved / limit) * 100).toFixed(1) : "0";
            const thisPctStr = limit > 0 ? ((thisEstimated / limit) * 100).toFixed(1) : "0";
            const remainingPctStr = limit > 0 ? ((remainingTokens / limit) * 100).toFixed(1) : "0";

            const segments = [
              {
                id: "real",
                value: realUsed,
                colorClass: "bg-sky-500",
                customTooltip: `Zużyte: ${realUsed.toLocaleString()} (${realPctStr}%)`,
              },
              {
                id: "other",
                value: otherReserved,
                colorClass: "bg-amber-500",
                customTooltip: `Inne zlecenia: ${otherReserved.toLocaleString()} (${otherReservedPctStr}%)`,
              },
              {
                id: "thisItem",
                value: thisEstimated,
                colorClass: "bg-indigo-500",
                customTooltip: `${itemLabel}: ${thisEstimated.toLocaleString()} (${thisPctStr}%)`,
              },
            ];

            return (
              <MultiSegmentProgressBar
                segments={segments}
                totalValue={limit}
                heightClass="h-3.5"
                showRemainingSegment
                remainingSegmentTooltip={`Wolne: ${remainingTokens.toLocaleString()} (${remainingPctStr}%)`}
              />
            );
          })()}
        </div>
      ) : null}
    </div>
  );
}
