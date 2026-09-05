export function getScoreColorClass(
    percentage: number | null | undefined,
): string {
    if (percentage === null || percentage === undefined) {
        return "text-muted-foreground/70";
    }
    if (percentage >= 100) {
        return "text-emerald-400";
    }
    return "text-foreground";
}

export function getScoreBadgeColorClass(
    percentage: number | null | undefined,
): string {
    if (percentage !== null && percentage !== undefined && percentage >= 100) {
        return "bg-emerald-500/15 text-emerald-400 border-emerald-500/30";
    }
    return "bg-secondary text-foreground border-border";
}
