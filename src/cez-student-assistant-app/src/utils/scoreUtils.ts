export function getScoreColorClass(
    percentage: number | null | undefined,
): string {
    if (percentage === null || percentage === undefined) {
        return "text-muted-foreground/70";
    }
    if (percentage <= 50) {
        return "text-rose-500 dark:text-rose-400";
    }
    if (percentage <= 79) {
        return "text-amber-500 dark:text-amber-400";
    }
    if (percentage < 100) {
        return "text-emerald-400 dark:text-emerald-300";
    }
    return "text-green-500 dark:text-green-400";
}

export function getScoreBadgeColorClass(
    percentage: number | null | undefined,
): string {
    if (percentage === null || percentage === undefined || percentage <= 50) {
        return "bg-rose-500/15 text-rose-600 dark:text-rose-400 border-rose-500/30";
    }
    if (percentage <= 80) {
        return "bg-amber-500/15 text-amber-600 dark:text-amber-400 border-amber-500/30";
    }
    if (percentage < 100) {
        return "bg-emerald-500/15 text-emerald-600 dark:text-emerald-300 border-emerald-500/30";
    }
    return "bg-green-500/15 text-green-600 dark:text-green-400 border-green-500/30";
}
