import { useContext } from "react";
import { useTranslation } from "react-i18next";
import { CourseContext } from "../contexts/CourseContext";
import { courseService, cezService, UnauthorizedError } from "../services";
import { useAuth } from "./useAuth";
import { useUI } from "./useUI";

export function useCourse() {
  const ctx = useContext(CourseContext);
  if (!ctx) {
    throw new Error("useCourse must be used within a CourseProvider");
  }

  const { t } = useTranslation();
  const { handleLogout, setIsCezConnected, setLastCezSync } = useAuth();
  const { setError, setSyncing } = useUI();



  const refreshCourses = async (pageNumber: number = 1, pageSize: number = 100, searchTerm?: string) => {
    try {
      const pagedResult = await courseService.getCourses(pageNumber, pageSize, searchTerm);
      ctx.setCourses(pagedResult.items);
      return pagedResult;
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        throw err;
      }
      setError(t("common.errorConnection"));
    }
  };

  const handleSyncCourses = async () => {
    setSyncing(true);
    try {
      await cezService.syncCourses();
      await refreshCourses();
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch (cezErr) {
        console.debug("[useCourse] CEZ status fallback on sync:", cezErr);
      }
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        return;
      }
      setError(err.message || t("common.errorConnection"));
    } finally {
      setSyncing(false);
    }
  };

  const createCourse = async (name: string, description?: string) => {
    try {
      await courseService.createCourse(name, description);
      await refreshCourses();
    } catch (err: any) {
      if (err instanceof UnauthorizedError) {
        handleLogout();
        throw err;
      }
      throw err;
    }
  };

  return {
    courses: ctx.courses,
    setCourses: ctx.setCourses,
    refreshCourses,
    handleSyncCourses,
    createCourse,
  };
}
