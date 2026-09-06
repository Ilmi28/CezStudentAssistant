import { useTranslation } from "react-i18next";
import { AlertTriangle } from "lucide-react";
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
  const { t } = useTranslation();

  if (!hasFiles) return null;

  return (
    <div className="space-y-2 pt-1 min-h-[68px]">
      <span className="block text-xs font-semibold text-foreground tracking-wide">
        Szacowanie zużycia
      </span>

      {loading && !estimation ? (
        <div className="space-y-1.5 pt-0.5">
          <div className="h-4 w-12 animate-pulse bg-muted rounded" />
          <div className="h-3.5 w-full animate-pulse bg-muted rounded-full" />
        </div>
      ) : estimation ? (
        <div className="space-y-1.5">
          <div className="flex justify-between items-center">
            <span
              className={`text-sm font-bold ${
                estimation.canGenerate ? "text-foreground" : "text-destructive"
              }`}
            >
              ~{estimation.estimatedDailyUsagePercentage}%
            </span>

            {!estimation.canGenerate && (
              <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-destructive bg-destructive/10 border border-destructive/20 px-2 py-0.5 rounded-md">
                <AlertTriangle size={13} className="shrink-0" />
                {t("courseDetails.limitExceeded", "Przekroczono limit dzienny")}
              </span>
            )}
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
                animated: false,
              },
              {
                id: "other",
                value: otherReserved,
                colorClass: "bg-amber-500",
                customTooltip: `Inne zlecenia: ${otherReserved.toLocaleString()} (${otherReservedPctStr}%)`,
                animated: false,
              },
              {
                id: "thisItem",
                value: thisEstimated,
                colorClass: estimation.canGenerate ? "bg-indigo-500" : "bg-rose-500",
                customTooltip: `${itemLabel}: ${thisEstimated.toLocaleString()} (${thisPctStr}%)`,
                animated: true,
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
