import { Routes, Route, Navigate } from "react-router-dom";
import { useAuth, useCourse, useQuiz, useUI } from "./hooks";

import DashboardLayout from "./layouts/DashboardLayout";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import CezLoginPage from "./pages/CezLoginPage";
import HomePage from "./pages/HomePage";
import CoursesPage from "./pages/CoursesPage";
import QuizzesPage from "./pages/QuizzesPage";
import CourseDetailsPage from "./pages/CourseDetailsPage";
import QuizDetailsPage from "./pages/QuizDetailsPage";
import QuizSolverPage from "./pages/QuizSolverPage";
import PreferencesPage from "./pages/PreferencesPage";

export default function AppRoutes() {
  const { isAuthenticated, isCezConnected, lastCezSync, handleLoginSuccess } = useAuth();
  const { courses, handleSyncCourses, createCourse } = useCourse();
  const { quizzes } = useQuiz();
  const { syncing, setError, setSuccess } = useUI();

  return (
    <Routes>
      {/* Public */}
      <Route path="/login"     element={isAuthenticated ? <Navigate to="/home" replace /> : <LoginPage     onLoginSuccess={handleLoginSuccess} setError={setError} setSuccess={setSuccess} />} />
      <Route path="/register"  element={isAuthenticated ? <Navigate to="/home" replace /> : <RegisterPage  setError={setError} setSuccess={setSuccess} />} />
      <Route path="/login-cez" element={isAuthenticated ? <Navigate to="/home" replace /> : <CezLoginPage  onLoginSuccess={handleLoginSuccess} setError={setError} setSuccess={setSuccess} />} />

      {/* Protected */}
      <Route element={isAuthenticated ? <DashboardLayout /> : <Navigate to="/login" replace />}>
        <Route path="/home"       element={<HomePage courses={courses} quizzes={quizzes} />} />
        <Route path="/courses"    element={<CoursesPage courses={courses} syncing={syncing} onSyncCourses={handleSyncCourses} onCreateCourse={createCourse} isCezConnected={isCezConnected} lastCezSync={lastCezSync} />} />
        <Route path="/quizzes"    element={<QuizzesPage quizzes={quizzes} />} />
        <Route path="/course/:id" element={<CourseDetailsPage setError={setError} setSuccess={setSuccess} />} />
        <Route path="/quiz/:id"   element={<QuizDetailsPage setError={setError} />} />
        <Route path="/quiz/:id/solve" element={<QuizSolverPage setError={setError} />} />
        <Route path="/quiz/attempt/:attemptId" element={<QuizSolverPage setError={setError} />} />
        <Route path="/preferences" element={<PreferencesPage />} />
      </Route>

      {/* Catch-all */}
      <Route path="/" element={<Navigate to={isAuthenticated ? "/home" : "/login"} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated ? "/home" : "/login"} replace />} />
    </Routes>
  );
}
