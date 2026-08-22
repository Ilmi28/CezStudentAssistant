import { useState, useEffect } from "react";

export interface CountdownResult {
  secondsLeft: number;
  formatted: string;
  isExpired: boolean;
  isTimeLow: boolean;
}

export function formatTimeLeft(seconds: number): string {
  if (seconds <= 0) return "00:00";
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hours > 0) {
    const hStr = hours.toString().padStart(2, "0");
    const mStr = minutes.toString().padStart(2, "0");
    const sStr = secs.toString().padStart(2, "0");
    return `${hStr}:${mStr}:${sStr}`;
  }

  const mStr = minutes.toString().padStart(2, "0");
  const sStr = secs.toString().padStart(2, "0");
  return `${mStr}:${sStr}`;
}

export function calculateSecondsLeft(expiresAt: string | Date | null | undefined): number {
  if (!expiresAt) return 0;
  const targetDate = new Date(expiresAt).getTime();
  const now = Date.now();
  const diffMs = targetDate - now;
  return Math.max(0, Math.floor(diffMs / 1000));
}

export function useCountdown(
  expiresAt: string | Date | null | undefined,
  onExpire?: () => void
): CountdownResult {
  const [secondsLeft, setSecondsLeft] = useState(() => calculateSecondsLeft(expiresAt));

  useEffect(() => {
    if (!expiresAt) {
      setSecondsLeft(0);
      return;
    }

    const update = () => {
      const remaining = calculateSecondsLeft(expiresAt);
      setSecondsLeft(remaining);
      if (remaining <= 0 && onExpire) {
        onExpire();
      }
    };

    update();
    const interval = setInterval(update, 1000);

    return () => clearInterval(interval);
  }, [expiresAt]);

  const isExpired = !!expiresAt && secondsLeft <= 0;
  const isTimeLow = secondsLeft > 0 && secondsLeft <= 60;

  return {
    secondsLeft,
    formatted: formatTimeLeft(secondsLeft),
    isExpired,
    isTimeLow,
  };
}
