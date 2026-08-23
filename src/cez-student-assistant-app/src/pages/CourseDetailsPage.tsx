import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, FileText, ChevronDown, Download, Pencil, Trash2, Plus, Brain } from "lucide-react";
import { courseService, quizService, type CourseDetailsDto, type QuizDto } from "../services";
import EditCourseModal from "../components/EditCourseModal";
import ConfirmModal from "../components/ConfirmModal";
import UploadFileModal from "../components/UploadFileModal";
import GenerateQuizModal from "../components/GenerateQuizModal";
import QuizCard from "../components/QuizCard";
import LoadingScreen from "../components/LoadingScreen";

import { useQuiz } from "../hooks";

interface CourseDetailsPageProps {
  setError: (msg: string) => void;
}

export default function CourseDetailsPage({
  setError
}: CourseDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { refreshQuizzes } = useQuiz();

  const [selectedCourse, setSelectedCourse] = useState<CourseDetailsDto | null>(null);
  const [courseQuizzes, setCourseQuizzes] = useState<QuizDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFilesExpanded, setIsFilesExpanded] = useState(false);
  const [isQuizzesExpanded, setIsQuizzesExpanded] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleteFileModalOpen, setIsDeleteFileModalOpen] = useState(false);
  const [fileToDelete, setFileToDelete] = useState<{ id: string; name: string } | null>(null);
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [isGenerateQuizModalOpen, setIsGenerateQuizModalOpen] = useState(false);
  const [isNavigatingBack, setIsNavigatingBack] = useState(false);

  useEffect(() => {
    loadCourseDetailsAndQuizzes();
  }, [id]);

  const loadCourseDetailsAndQuizzes = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const [details, allQuizzes] = await Promise.all([
        courseService.getCourseDetails(id),
        quizService.getQuizzes().catch((quizErr) => {
          console.debug("[CourseDetailsPage] Failed to load quizzes:", quizErr);
          return [] as QuizDto[];
        })
      ]);
      setSelectedCourse(details);
      setCourseQuizzes(allQuizzes.filter((q) => q.courseId === id));
    } catch (err) {
      console.warn("[CourseDetailsPage] Failed to load details:", err);
      setError(t("common.genericError"));
      navigate("/courses");
    } finally {
      setLoading(false);
    }
  };

  const handleGoBack = () => {
    if (isNavigatingBack) return;
    setIsNavigatingBack(true);
    setTimeout(() => {
      navigate("/courses");
    }, 170);
  };

  const handleEditCourseSubmit = async (name: string, description?: string) => {
    if (!id) return;
    await courseService.updateCourse(id, name, description);
    const details = await courseService.getCourseDetails(id);
    setSelectedCourse(details);
  };

  const handleDeleteCourseConfirm = async () => {
    if (!id) return;
    await courseService.deleteCourse(id);
    navigate("/courses");
  };

  const handleFileUploadSubmit = async (file: File) => {
    if (!id) return;
    await courseService.uploadCourseFile(id, file);
    const details = await courseService.getCourseDetails(id);
    setSelectedCourse(details);
  };

  const handleGenerateQuizSubmit = async (
    questionCount: number,
    timeLimitMinutes?: number | null,
    additionalInstructions?: string,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null,
    questionCountPerAttempt?: number | null
  ) => {
    if (!id) return;
    await courseService.generateQuiz(
      id,
      questionCount,
      timeLimitMinutes,
      additionalInstructions,
      easyCount,
      mediumCount,
      hardCount,
      questionCountPerAttempt
    );
    await refreshQuizzes();
  };

  const handleFileDownload = async (fileId: string, fileName: string) => {
    if (!id) return;
    try {
      await courseService.downloadCourseFile(id, fileId, fileName);
    } catch (downloadErr) {
      console.warn("[CourseDetailsPage] File download failed:", downloadErr);
      setError(t("common.genericError"));
    }
  };

  const handleConfirmDeleteFile = async () => {
    if (!id || !fileToDelete) return;
    try {
      await courseService.deleteCourseFile(id, fileToDelete.id);
      const details = await courseService.getCourseDetails(id);
      setSelectedCourse(details);
    } catch (deleteErr) {
      console.warn("[CourseDetailsPage] File deletion failed:", deleteErr);
      setError(t("common.genericError"));
    } finally {
      setFileToDelete(null);
      setIsDeleteFileModalOpen(false);
    }
  };

  if (loading || !selectedCourse) {
    return <LoadingScreen message={t("courseDetails.loadingDetails")} />;
  }

  return (
    <div
      className={`max-w-4xl mx-auto space-y-6 ${
        isNavigatingBack
          ? "animate-slide-out-right"
          : "animate-in fade-in duration-300"
      }`}
    >
      {/* Header & Title */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <button
            type="button"
            onClick={handleGoBack}
            title={t("courseDetails.backBtn")}
            aria-label={t("courseDetails.backBtn")}
            className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer shrink-0"
          >
            <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
          </button>
          <div className="flex flex-col justify-center min-w-0">
            {selectedCourse.isCez && (
              <span className="self-start px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20 mb-1">
                {t("courses.tagCez")}
              </span>
            )}
            <h1 className="text-xl font-bold text-foreground truncate">
              {selectedCourse.name}
            </h1>
          </div>
        </div>

        {!selectedCourse.isCez && (
          <div className="flex items-center gap-2.5 shrink-0">
            <button
              type="button"
              onClick={() => setIsEditModalOpen(true)}
              title={t("courses.editCourseModalTitle")}
              aria-label={t("courses.editCourseModalTitle")}
              className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer"
            >
              <Pencil size={18} strokeWidth={2.25} className="shrink-0" />
            </button>

            <button
              type="button"
              onClick={() => setIsDeleteModalOpen(true)}
              title={t("courses.deleteCourseModalTitle")}
              aria-label={t("courses.deleteCourseModalTitle")}
              className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-destructive/40 hover:text-destructive transition-colors shadow-xs cursor-pointer"
            >
              <Trash2 size={18} strokeWidth={2.25} className="shrink-0" />
            </button>
          </div>
        )}
      </div>

      {/* Files List Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <div
          onClick={() => setIsFilesExpanded(!isFilesExpanded)}
          className="flex items-center justify-between cursor-pointer select-none group"
        >
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground group-hover:text-primary transition-colors">
            {t("courseDetails.filesTitle")} ({selectedCourse.files.length})
          </h3>
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setIsUploadModalOpen(true);
              }}
              title={t("courseDetails.uploadTitle")}
              aria-label={t("courseDetails.uploadTitle")}
              className="p-1.5 rounded-lg text-muted-foreground hover:text-primary hover:bg-muted transition-colors cursor-pointer"
            >
              <Plus size={20} />
            </button>
            <button
              type="button"
              className="text-muted-foreground group-hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-muted cursor-pointer"
            >
              <ChevronDown
                size={18}
                className={`transition-transform duration-300 ${isFilesExpanded ? "rotate-180" : ""}`}
              />
            </button>
          </div>
        </div>

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isFilesExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden">
            <div className="pt-3.5 border-t border-border mt-3.5">
              {selectedCourse.files.length === 0 ? (
                <div className="py-8 text-center">
                  <FileText size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                  <p className="text-xs text-muted-foreground">{t("courseDetails.noFiles")}</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {selectedCourse.files.map((file) => (
                    <div
                      key={file.id}
                      className="flex items-center justify-between p-3.5 rounded-xl bg-muted/40 border border-border hover:border-primary/30 transition-all"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-8 h-8 rounded-lg bg-card border border-border flex items-center justify-center text-primary flex-shrink-0">
                          <FileText size={15} />
                        </div>
                        <div className="min-w-0">
                          <div className="text-xs font-medium text-foreground truncate">{file.displayName}</div>
                          <div className="text-[11px] text-muted-foreground/50">{file.mimeType}</div>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 shrink-0 ml-2">
                        <button
                          type="button"
                          onClick={() => handleFileDownload(file.id, file.displayName)}
                          title={t("courseDetails.downloadFile")}
                          aria-label={t("courseDetails.downloadFile")}
                          className="p-2 rounded-lg text-muted-foreground hover:text-primary hover:bg-muted transition-colors cursor-pointer"
                        >
                          <Download size={18} />
                        </button>
                        {!selectedCourse.isCez && (
                          <button
                            type="button"
                            onClick={() => {
                              setFileToDelete({ id: file.id, name: file.displayName });
                              setIsDeleteFileModalOpen(true);
                            }}
                            title={t("courseDetails.deleteFile")}
                            aria-label={t("courseDetails.deleteFile")}
                            className="p-2 rounded-lg text-muted-foreground hover:text-destructive hover:bg-muted transition-colors cursor-pointer"
                          >
                            <Trash2 size={18} />
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Quizzes Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <div
          onClick={() => setIsQuizzesExpanded(!isQuizzesExpanded)}
          className="flex items-center justify-between cursor-pointer select-none group"
        >
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground group-hover:text-primary transition-colors">
            {t("courseDetails.quizzesTitle")} ({courseQuizzes.length})
          </h3>
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                setIsGenerateQuizModalOpen(true);
              }}
              title={t("courseDetails.generateTitle")}
              aria-label={t("courseDetails.generateTitle")}
              className="p-1.5 rounded-lg text-muted-foreground hover:text-primary hover:bg-muted transition-colors cursor-pointer"
            >
              <Plus size={20} />
            </button>
            <button
              type="button"
              className="text-muted-foreground group-hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-muted cursor-pointer"
            >
              <ChevronDown
                size={18}
                className={`transition-transform duration-300 ${isQuizzesExpanded ? "rotate-180" : ""}`}
              />
            </button>
          </div>
        </div>

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isQuizzesExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden">
            <div className="pt-3.5 border-t border-border mt-3.5">
              {courseQuizzes.length === 0 ? (
                <div className="py-8 text-center">
                  <Brain size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                  <p className="text-xs text-muted-foreground">{t("quizzes.noQuizzes")}</p>
                  <p className="text-[11px] text-muted-foreground/60 mt-1">{t("quizzes.noQuizzesSubtitle")}</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  {courseQuizzes.map((quiz, idx) => (
                    <QuizCard key={quiz.id} quiz={quiz} index={idx + 1} showCourseName={false} />
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <EditCourseModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        onSubmit={handleEditCourseSubmit}
        initialName={selectedCourse.name}
        initialDescription={selectedCourse.description}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        onConfirm={handleDeleteCourseConfirm}
        title={t("courses.deleteCourseModalTitle")}
        message={t("courses.deleteCourseConfirmMsg")}
        confirmBtnText={t("courses.deleteCourseBtn")}
        isDestructive
      />

      <ConfirmModal
        isOpen={isDeleteFileModalOpen}
        onClose={() => {
          setIsDeleteFileModalOpen(false);
          setFileToDelete(null);
        }}
        onConfirm={handleConfirmDeleteFile}
        title={t("courseDetails.deleteFileModalTitle")}
        message={t("courseDetails.deleteFileConfirmMsg")}
        confirmBtnText={t("courseDetails.deleteFileBtn")}
        isDestructive
      />

      <UploadFileModal
        isOpen={isUploadModalOpen}
        onClose={() => setIsUploadModalOpen(false)}
        onSubmit={handleFileUploadSubmit}
      />

      <GenerateQuizModal
        isOpen={isGenerateQuizModalOpen}
        onClose={() => setIsGenerateQuizModalOpen(false)}
        onSubmit={handleGenerateQuizSubmit}
        hasFiles={selectedCourse.files.length > 0}
        courseId={id || ""}
      />
    </div>
  );
}
