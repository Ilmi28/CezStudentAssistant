import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { authService, courseService, quizService, cezService, type CourseDto, type QuizDto } from "../services";

interface AppContextType {
  // Auth
  username: string | null;
  isAuthenticated: boolean;
  isAuthChecking: boolean;
  isCezConnected: boolean;
  lastCezSync: string | null;
  handleLoginSuccess: (user: string) => Promise<void>;
  handleLogout: () => void;

  // Data
  courses: CourseDto[];
  quizzes: QuizDto[];

  // UI state
  loading: boolean;
  syncing: boolean;
  darkMode: boolean;
  showCezModal: boolean;
  errorMsg: string | null;
  successMsg: string | null;

  // Actions
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
  setErrorMsg: (msg: string | null) => void;
  setSuccessMsg: (msg: string | null) => void;
  handleSyncCourses: () => Promise<void>;
  handleCezLinkSubmit: (cezUser: string, cezPass: string) => Promise<void>;
  handleRefreshLists: () => Promise<void>;
  setShowCezModal: (open: boolean) => void;
  toggleDarkMode: () => void;
}

const AppContext = createContext<AppContextType | null>(null);

export function useApp(): AppContextType {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error("useApp must be used within AppProvider");
  return ctx;
}

export function AppProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [username, setUsername] = useState<string | null>(null);
  const [courses, setCourses] = useState<CourseDto[]>([]);
  const [quizzes, setQuizzes] = useState<QuizDto[]>([]);
  const [isCezConnected, setIsCezConnected] = useState<boolean>(false);
  const [lastCezSync, setLastCezSync] = useState<string | null>(null);

  const [darkMode, setDarkMode] = useState<boolean>(() => {
    const stored = localStorage.getItem("darkMode");
    return stored !== null ? stored === "true" : true;
  });

  const [isAuthChecking, setIsAuthChecking] = useState(true);
  const [loading, setLoading] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [showCezModal, setShowCezModal] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Dark mode sync
  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add("dark");
      localStorage.setItem("darkMode", "true");
    } else {
      document.documentElement.classList.remove("dark");
      localStorage.setItem("darkMode", "false");
    }
  }, [darkMode]);

  // Auth check on mount
  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      const courseList = await courseService.getCourses();
      setCourses(courseList);
      const quizList = await quizService.getQuizzes();
      setQuizzes(quizList);
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch {
        // Fallback if status is not available
      }
      const storedUser = localStorage.getItem("username") || "Student";
      setUsername(storedUser);
    } catch (err: any) {
      const msg = err.message || "";
      if (
        msg === "UNAUTHORIZED" ||
        msg === "User is not authenticated." ||
        msg.includes("authenticated") ||
        msg.includes("401")
      ) {
        setUsername(null);
      } else {
        setError(t("common.errorConnection"));
      }
    } finally {
      setIsAuthChecking(false);
    }
  };

  const setError = (msg: string) => {
    setErrorMsg(msg);
    setTimeout(() => setErrorMsg(null), 5000);
  };

  const setSuccess = (msg: string) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(null), 5000);
  };

  const handleLoginSuccess = async (user: string) => {
    localStorage.setItem("username", user);
    setUsername(user);
    setLoading(true);
    try {
      const courseList = await courseService.getCourses();
      setCourses(courseList);
      const quizList = await quizService.getQuizzes();
      setQuizzes(quizList);
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch {
        // Fallback
      }
      navigate("/home");
    } catch {
      setError(t("common.errorConnection"));
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
    } catch {
      // Ignore network/server errors on logout
    }
    localStorage.removeItem("username");
    setUsername(null);
    setCourses([]);
    setQuizzes([]);
    setIsCezConnected(false);
    setLastCezSync(null);
    document.cookie = "accessToken=; Max-Age=0; path=/;";
    document.cookie = "refreshToken=; Max-Age=0; path=/;";
    navigate("/login");
  };

  const handleSyncCourses = async () => {
    setSyncing(true);
    setSuccess(t("courses.syncBtnLoading") + "...");
    try {
      await cezService.syncCourses();
      setSuccess(t("common.syncSuccess"));
      const courseList = await courseService.getCourses();
      setCourses(courseList);
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch {
        // Fallback
      }
    } catch (err: any) {
      setError(err.message || t("common.errorConnection"));
    } finally {
      setSyncing(false);
    }
  };

  const handleCezLinkSubmit = async (cezUser: string, cezPass: string) => {
    setLoading(true);
    try {
      await authService.loginCez(cezUser, cezPass);
      setIsCezConnected(true);
      setSuccess(t("common.syncSuccess"));
      setShowCezModal(false);
      handleSyncCourses();
    } catch (err: any) {
      setError(err.message || t("common.errorConnection"));
    } finally {
      setLoading(false);
    }
  };

  const handleRefreshLists = async () => {
    setLoading(true);
    try {
      const courseList = await courseService.getCourses();
      setCourses(courseList);
      const quizList = await quizService.getQuizzes();
      setQuizzes(quizList);
      try {
        const cezStatus = await cezService.getCezStatus();
        setIsCezConnected(cezStatus.isConnected);
        setLastCezSync(cezStatus.lastSyncAt);
      } catch {
        // Fallback
      }
      setSuccess(t("common.refreshSuccess"));
    } catch {
      setError(t("common.errorConnection"));
    } finally {
      setLoading(false);
    }
  };

  const toggleDarkMode = () => setDarkMode(prev => !prev);

  const value: AppContextType = {
    username,
    isAuthenticated: username !== null,
    isAuthChecking,
    isCezConnected,
    lastCezSync,
    handleLoginSuccess,
    handleLogout,
    courses,
    quizzes,
    loading,
    syncing,
    darkMode,
    showCezModal,
    errorMsg,
    successMsg,
    setError,
    setSuccess,
    setErrorMsg,
    setSuccessMsg,
    handleSyncCourses,
    handleCezLinkSubmit,
    handleRefreshLists,
    setShowCezModal,
    toggleDarkMode,
  };

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}
