import { Routes, Route, Navigate } from "react-router-dom";
import { useApp } from "./contexts/AppContext";

import DashboardLayout from "./layouts/DashboardLayout";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import CezLoginPage from "./pages/CezLoginPage";
import HomePage from "./pages/HomePage";
import CoursesPage from "./pages/CoursesPage";
import QuizzesPage from "./pages/QuizzesPage";
import CourseDetailsPage from "./pages/CourseDetailsPage";
import QuizSolverPage from "./pages/QuizSolverPage";

export default function AppRoutes() {
  const {
    isAuthenticated,
    isCezConnected,
    lastCezSync,
    handleLoginSuccess,
    setError,
    setSuccess,
    courses,
    quizzes,
    syncing,
    handleSyncCourses,
  } = useApp();

  return (
    <Routes>
      {/* Public */}
      <Route path="/login"     element={isAuthenticated ? <Navigate to="/home" replace /> : <LoginPage     onLoginSuccess={handleLoginSuccess} setError={setError} setSuccess={setSuccess} />} />
      <Route path="/register"  element={isAuthenticated ? <Navigate to="/home" replace /> : <RegisterPage  setError={setError} setSuccess={setSuccess} />} />
      <Route path="/login-cez" element={isAuthenticated ? <Navigate to="/home" replace /> : <CezLoginPage  onLoginSuccess={handleLoginSuccess} setError={setError} setSuccess={setSuccess} />} />

      {/* Protected */}
      <Route element={isAuthenticated ? <DashboardLayout /> : <Navigate to="/login" replace />}>
        <Route path="/home"       element={<HomePage courses={courses} quizzes={quizzes} />} />
        <Route path="/courses"    element={<CoursesPage courses={courses} syncing={syncing} onSyncCourses={handleSyncCourses} isCezConnected={isCezConnected} lastCezSync={lastCezSync} />} />
        <Route path="/quizzes"    element={<QuizzesPage quizzes={quizzes} />} />
        <Route path="/course/:id" element={<CourseDetailsPage setError={setError} setSuccess={setSuccess} />} />
        <Route path="/quiz/:id"   element={<QuizSolverPage setError={setError} />} />
      </Route>

      {/* Catch-all */}
      <Route path="/" element={<Navigate to={isAuthenticated ? "/home" : "/login"} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated ? "/home" : "/login"} replace />} />
    </Routes>
  );
}
