import { useState, useEffect, useCallback } from "react";
import { userService } from "../services";
import type { UserUsageDto } from "../types";

export function useUser() {
  const [usage, setUsage] = useState<UserUsageDto | null>(null);
  const [loadingUsage, setLoadingUsage] = useState<boolean>(true);
  const [usageError, setUsageError] = useState<string | null>(null);

  const fetchUserUsage = useCallback(async () => {
    setLoadingUsage(true);
    setUsageError(null);
    try {
      const data = await userService.getUserUsage();
      setUsage(data);
    } catch (err) {
      console.warn("[useUser] Failed to fetch user usage:", err);
      setUsageError("Failed to fetch user usage data.");
    } finally {
      setLoadingUsage(false);
    }
  }, []);

  useEffect(() => {
    fetchUserUsage();
  }, [fetchUserUsage]);

  return {
    usage,
    loadingUsage,
    usageError,
    fetchUserUsage,
  };
}
