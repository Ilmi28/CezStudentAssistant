import { useState, useEffect, useCallback } from "react";
import { userService } from "../services";
import type { DashboardStatsDto, RecentActivityDto } from "../types";

export function useDashboard() {
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [loadingStats, setLoadingStats] = useState<boolean>(true);
  const [statsError, setStatsError] = useState<string | null>(null);

  const [recentActivity, setRecentActivity] = useState<RecentActivityDto[]>([]);
  const [loadingActivity, setLoadingActivity] = useState<boolean>(true);
  const [activityError, setActivityError] = useState<string | null>(null);

  const fetchDashboardStats = useCallback(async () => {
    setLoadingStats(true);
    setStatsError(null);
    try {
      const data = await userService.getDashboardStats();
      setStats(data);
    } catch (err) {
      console.warn("[useDashboard] Failed to fetch dashboard stats:", err);
      setStatsError("Failed to fetch dashboard statistics.");
    } finally {
      setLoadingStats(false);
    }
  }, []);

  const fetchRecentActivity = useCallback(async (limit: number = 6) => {
    setLoadingActivity(true);
    setActivityError(null);
    try {
      const data = await userService.getRecentActivity(limit);
      setRecentActivity(data);
    } catch (err) {
      console.warn("[useDashboard] Failed to fetch recent activity:", err);
      setActivityError("Failed to fetch recent activity.");
    } finally {
      setLoadingActivity(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboardStats();
    fetchRecentActivity();
  }, [fetchDashboardStats, fetchRecentActivity]);

  return {
    stats,
    loadingStats,
    statsError,
    recentActivity,
    loadingActivity,
    activityError,
    refreshStats: fetchDashboardStats,
    refreshActivity: fetchRecentActivity,
  };
}
