import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { api, type CourseDto, type QuizDto } from "../services/api";

interface AppContextType {
  // Auth
  username: string | null;
  isAuthenticated: boolean;
  isAuthChecking: boolean;
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

  const [darkMode, setDarkMode] = useState<boolean>(() => {
    return localStorage.getItem("darkMode") === "true";
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
      const courseList = await api.getCourses();
      setCourses(courseList);
      const quizList = await api.getQuizzes();
      setQuizzes(quizList);
      const storedUser = localStorage.getItem("username") || "Student";
      setUsername(storedUser);
    } catch (err: any) {
      if (err.message === "UNAUTHORIZED") {
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
      const courseList = await api.getCourses();
      setCourses(courseList);
      const quizList = await api.getQuizzes();
      setQuizzes(quizList);
      navigate("/home");
    } catch {
      setError(t("common.errorConnection"));
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("username");
    setUsername(null);
    document.cookie = "accessToken=; Max-Age=0; path=/;";
    document.cookie = "refreshToken=; Max-Age=0; path=/;";
    navigate("/login");
  };

  const handleSyncCourses = async () => {
    setSyncing(true);
    setSuccess(t("courses.syncBtnLoading") + "...");
    try {
      await api.syncCourses();
      setSuccess(t("common.syncSuccess"));
      const courseList = await api.getCourses();
      setCourses(courseList);
    } catch (err: any) {
      setError(err.message || t("common.errorConnection"));
    } finally {
      setSyncing(false);
    }
  };

  const handleCezLinkSubmit = async (cezUser: string, cezPass: string) => {
    setLoading(true);
    try {
      await api.loginCez(cezUser, cezPass);
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
      const courseList = await api.getCourses();
      setCourses(courseList);
      const quizList = await api.getQuizzes();
      setQuizzes(quizList);
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
