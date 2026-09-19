export function getUserLocale(): string | undefined {
  return typeof navigator !== "undefined" && navigator.language ? navigator.language : undefined;
}

export function formatDateTime(
  dateValue: string | Date | number | null | undefined,
  options: Intl.DateTimeFormatOptions = { dateStyle: "short", timeStyle: "short" }
): string {
  if (!dateValue) return "";
  const date = typeof dateValue === "string" || typeof dateValue === "number" ? new Date(dateValue) : dateValue;
  if (isNaN(date.getTime())) return "";
  return date.toLocaleString(getUserLocale(), options);
}

export function formatDate(
  dateValue: string | Date | number | null | undefined,
  options: Intl.DateTimeFormatOptions = { dateStyle: "medium" }
): string {
  if (!dateValue) return "";
  const date = typeof dateValue === "string" || typeof dateValue === "number" ? new Date(dateValue) : dateValue;
  if (isNaN(date.getTime())) return "";
  return date.toLocaleDateString(getUserLocale(), options);
}

export function formatTime(
  dateValue: string | Date | number | null | undefined,
  options: Intl.DateTimeFormatOptions = { hour: "2-digit", minute: "2-digit" }
): string {
  if (!dateValue) return "";
  const date = typeof dateValue === "string" || typeof dateValue === "number" ? new Date(dateValue) : dateValue;
  if (isNaN(date.getTime())) return "";
  return date.toLocaleTimeString(getUserLocale(), options);
}
