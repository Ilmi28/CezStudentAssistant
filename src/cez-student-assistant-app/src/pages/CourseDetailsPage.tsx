import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, FileText, ChevronDown, Download, Pencil, Trash2, Plus, Brain } from "lucide-react";
import { courseService, type CourseDetailsDto, type CourseResourceDto } from "../services";
import { quizService } from "../services/quizService";
import { flashcardService } from "../services/flashcardService";
import { signalRService } from "../services/signalRService";
import type { QuizDto } from "../types/quizTypes";
import type { FlashcardDeckDto } from "../types/flashcardTypes";
import {
  EditCourseModal,
  ConfirmModal,
  UploadFileModal,
  GenerateQuizModal,
  GenerateFlashcardsModal,
  QuizCard,
  FlashcardDeckCard,
  LoadingScreen,
  SecondaryButton,
  Badge,
  CoursePreparationCard,
} from "../components";

interface CourseDetailsPageProps {
  setError: (msg: string) => void;
}

interface AccordionHeaderProps {
  title: string;
  count: number;
  isExpanded: boolean;
  onToggle: () => void;
  onAddClick?: () => void;
  addTitle?: string;
}

function AccordionHeader({
  title,
  count,
  isExpanded,
  onToggle,
  onAddClick,
  addTitle,
}: AccordionHeaderProps) {
  return (
    <div
      onClick={onToggle}
      className="flex items-center justify-between cursor-pointer select-none group py-1"
    >
      <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
        {title} ({count})
      </h3>
      <div className="flex items-center gap-1">
        {onAddClick && (
          <SecondaryButton
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onAddClick();
            }}
            title={addTitle}
            aria-label={addTitle}
            icon={<Plus size={18} strokeWidth={2.25} />}
            className="w-9 h-9 p-0 flex items-center justify-center shrink-0"
          />
        )}
        <div
          className="w-9 h-9 flex items-center justify-center text-muted-foreground group-hover:text-foreground transition-colors cursor-pointer shrink-0"
        >
          <ChevronDown
            size={18}
            className={`transition-transform duration-300 ${isExpanded ? "rotate-180" : ""}`}
          />
        </div>
      </div>
    </div>
  );
}

export default function CourseDetailsPage({
  setError
}: CourseDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [selectedCourse, setSelectedCourse] = useState<CourseDetailsDto | null>(null);
  const [courseFiles, setCourseFiles] = useState<CourseResourceDto[]>([]);
  const [courseQuizzes, setCourseQuizzes] = useState<QuizDto[]>([]);
  const [decks, setDecks] = useState<FlashcardDeckDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFilesExpanded, setIsFilesExpanded] = useState(false);
  const [isQuizzesExpanded, setIsQuizzesExpanded] = useState(false);
  const [isFlashcardsExpanded, setIsFlashcardsExpanded] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleteFileModalOpen, setIsDeleteFileModalOpen] = useState(false);
  const [fileToDelete, setFileToDelete] = useState<{ id: string; name: string } | null>(null);
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [isGenerateQuizModalOpen, setIsGenerateQuizModalOpen] = useState(false);
  const [isGenerateFlashcardsModalOpen, setIsGenerateFlashcardsModalOpen] = useState(false);

  useEffect(() => {
    loadCourseDetailsAndQuizzes();

    signalRService.startConnection();
    const unsubscribe = signalRService.subscribeJobStatus((_jobId, status) => {
      if (status === "Succeeded" || status === "Failed") {
        if (id) {
          quizService.getQuizzes(id).then(setCourseQuizzes).catch(console.warn);
          flashcardService.getFlashcardDecks(id).then(setDecks).catch(console.warn);
        }
      }
    });

    return () => {
      unsubscribe();
    };
  }, [id]);

  const loadCourseDetailsAndQuizzes = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const [details, filesData, quizzesData, decksData] = await Promise.all([
        courseService.getCourseDetails(id),
        courseService.getCourseFiles(id),
        quizService.getQuizzes(id),
        flashcardService.getFlashcardDecks(id),
      ]);
      setSelectedCourse(details);
      setCourseFiles(filesData);
      setCourseQuizzes(quizzesData);
      setDecks(decksData);
    } catch (err) {
      console.warn("[CourseDetailsPage] Failed to load details:", err);
      setError(t("common.genericError"));
      navigate("/courses");
    } finally {
      setLoading(false);
    }
  };

  const handleGoBack = () => {
    navigate("/courses");
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
    const filesData = await courseService.getCourseFiles(id);
    setCourseFiles(filesData);
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
    const quizzesData = await quizService.getQuizzes(id);
    setCourseQuizzes(quizzesData);
  };

  const handleGenerateFlashcardsSubmit = async (
    cardCount: number,
    additionalInstructions?: string,
    easyCount?: number | null,
    mediumCount?: number | null,
    hardCount?: number | null
  ) => {
    if (!id) return;
    await flashcardService.generateFlashcards(
      id,
      cardCount,
      additionalInstructions,
      easyCount,
      mediumCount,
      hardCount
    );
    const decksData = await flashcardService.getFlashcardDecks(id);
    setDecks(decksData);
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
      const filesData = await courseService.getCourseFiles(id);
      setCourseFiles(filesData);
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
    <div className="max-w-4xl mx-auto space-y-6 animate-in fade-in duration-300">
      {/* Header & Title */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3.5 min-w-0">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("courseDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0">
            {selectedCourse.isCez && (
              <Badge variant="secondary" className="self-start mb-1">
                {t("courses.tagCez")}
              </Badge>
            )}
            <h1 className="text-xl font-bold text-foreground truncate">
              {selectedCourse.name}
            </h1>
          </div>
        </div>

        {!selectedCourse.isCez && (
          <div className="flex items-center gap-2.5 shrink-0">
            <SecondaryButton
              type="button"
              onClick={() => setIsEditModalOpen(true)}
              icon={<Pencil size={15} strokeWidth={2.25} />}
              className="py-2.5 px-3.5 text-xs font-semibold"
            >
              {t("quizDetails.editBtn")}
            </SecondaryButton>
            <SecondaryButton
              type="button"
              onClick={() => setIsDeleteModalOpen(true)}
              icon={<Trash2 size={15} strokeWidth={2.25} className="text-rose-400" />}
              className="py-2.5 px-3.5 text-xs font-semibold"
            >
              {t("courses.deleteCourseBtn")}
            </SecondaryButton>
          </div>
        )}
      </div>

      {/* Course Preparation Stats Card */}
      <CoursePreparationCard course={selectedCourse} />

      {/* Files List Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <AccordionHeader
          title={t("courseDetails.filesTitle")}
          count={courseFiles.length}
          isExpanded={isFilesExpanded}
          onToggle={() => setIsFilesExpanded(!isFilesExpanded)}
          onAddClick={() => setIsUploadModalOpen(true)}
          addTitle={t("courseDetails.uploadTitle")}
        />

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isFilesExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden p-1 -m-1">
            <div className="pt-3.5 border-t border-border mt-3.5 px-0.5">
              {courseFiles.length === 0 ? (
                <div className="py-8 text-center">
                  <FileText size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                  <p className="text-xs text-muted-foreground">{t("courseDetails.noFiles")}</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {courseFiles.map((file) => (
                    <div
                      key={file.id}
                      className="flex items-center justify-between p-3.5 rounded-xl bg-muted/40 border border-border transition-all"
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
        <AccordionHeader
          title={t("courseDetails.quizzesTitle")}
          count={courseQuizzes.length}
          isExpanded={isQuizzesExpanded}
          onToggle={() => setIsQuizzesExpanded(!isQuizzesExpanded)}
          onAddClick={() => setIsGenerateQuizModalOpen(true)}
          addTitle={t("courseDetails.generateTitle")}
        />

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isQuizzesExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden p-1 -m-1">
            <div className="pt-3.5 border-t border-border mt-3.5 px-0.5">
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

      {/* Flashcards Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <AccordionHeader
          title="Fiszki"
          count={decks.length}
          isExpanded={isFlashcardsExpanded}
          onToggle={() => setIsFlashcardsExpanded(!isFlashcardsExpanded)}
          onAddClick={() => setIsGenerateFlashcardsModalOpen(true)}
          addTitle="Wygeneruj fiszki"
        />

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isFlashcardsExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden p-1 -m-1">
            <div className="pt-3.5 border-t border-border mt-3.5 px-0.5">
              {decks.length === 0 ? (
                <div className="py-8 text-center">
                  <FileText size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                  <p className="text-xs text-muted-foreground">{t("flashcards.noCourseDecks")}</p>
                  <p className="text-[11px] text-muted-foreground/60 mt-1">{t("flashcards.noCourseDecksSubtitle")}</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  {decks.map((deck, idx) => (
                    <FlashcardDeckCard key={deck.id} deck={deck} index={idx + 1} showCourseName={false} />
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
        hasFiles={courseFiles.length > 0}
        courseId={id || ""}
      />

      <GenerateFlashcardsModal
        isOpen={isGenerateFlashcardsModalOpen}
        onClose={() => setIsGenerateFlashcardsModalOpen(false)}
        onSubmit={handleGenerateFlashcardsSubmit}
        hasFiles={courseFiles.length > 0}
        courseId={id || ""}
      />
    </div>
  );
}
