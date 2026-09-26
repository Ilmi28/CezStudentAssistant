import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, FileText, ChevronDown, Download, Pencil, Trash2, Plus, Eye, EyeOff } from "lucide-react";
import { courseService, chatService, type CourseDetailsDto, type CourseResourceDto } from "../services";
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
  ChatThreadCard,
  LoadingScreen,
  SecondaryButton,
  CoursePreparationCard,
} from "../components";
import { useCourseChat } from "../hooks/useCourseChat";

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
  const [isChatExpanded, setIsChatExpanded] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleteFileModalOpen, setIsDeleteFileModalOpen] = useState(false);
  const [fileToDelete, setFileToDelete] = useState<{ id: string; name: string } | null>(null);
  const [quizToDelete, setQuizToDelete] = useState<{ id: string; name: string } | null>(null);
  const [deckToDelete, setDeckToDelete] = useState<{ id: string; name: string } | null>(null);
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [isGenerateQuizModalOpen, setIsGenerateQuizModalOpen] = useState(false);
  const [isGenerateFlashcardsModalOpen, setIsGenerateFlashcardsModalOpen] = useState(false);

  const { threads, deleteThread } = useCourseChat(id || "");

  const handleCreateChatThread = async () => {
    if (!id) return;
    try {
      setLoading(true);
      const files = await courseService.getCourseFiles(id);
      const defaultAttachedIds = files.filter((f) => !f.isHidden).map((f) => f.id);
      const created = await chatService.createChatThread({
        courseId: id,
        attachedResourceIds: defaultAttachedIds,
      });
      navigate(`/chats/${created.id}`, { state: { fromPath: location.pathname } });
    } catch (err) {
      console.warn("[CourseDetailsPage] Failed to create chat thread:", err);
      setError(t("common.genericError"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCourseDetailsAndQuizzes();

    signalRService.startConnection();
    const unsubscribe = signalRService.subscribeJobStatus((_jobId, status) => {
      if (status === "Succeeded" || status === "Failed") {
        if (id) {
          quizService.getQuizzes(id).then((res) => setCourseQuizzes(res.items)).catch(console.warn);
          flashcardService.getFlashcardDecks(id).then((res) => setDecks(res.items)).catch(console.warn);
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
      setCourseQuizzes(quizzesData.items);
      setDecks(decksData.items);
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
    setCourseQuizzes(quizzesData.items);
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
    setDecks(decksData.items);
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

  const handleToggleFileVisibility = async (fileId: string) => {
    if (!id) return;
    try {
      await courseService.toggleCourseFileVisibility(id, fileId);
      const filesData = await courseService.getCourseFiles(id);
      setCourseFiles(filesData);
    } catch (toggleErr) {
      console.warn("[CourseDetailsPage] File visibility toggle failed:", toggleErr);
      setError(t("common.genericError"));
    }
  };

  const handleConfirmDeleteQuiz = async () => {
    if (!id || !quizToDelete) return;
    try {
      await quizService.deleteQuiz(quizToDelete.id);
      const quizzesData = await quizService.getQuizzes(id);
      setCourseQuizzes(quizzesData.items);
    } catch (deleteErr) {
      console.warn("[CourseDetailsPage] Quiz deletion failed:", deleteErr);
      setError(t("common.genericError"));
    } finally {
      setQuizToDelete(null);
    }
  };

  const handleConfirmDeleteDeck = async () => {
    if (!id || !deckToDelete) return;
    try {
      await flashcardService.deleteFlashcardDeck(deckToDelete.id);
      const decksData = await flashcardService.getFlashcardDecks(id);
      setDecks(decksData.items);
    } catch (deleteErr) {
      console.warn("[CourseDetailsPage] Flashcard deck deletion failed:", deleteErr);
      setError(t("common.genericError"));
    } finally {
      setDeckToDelete(null);
    }
  };

  if (loading || !selectedCourse) {
    return <LoadingScreen message={t("courseDetails.loadingDetails")} />;
  }

  return (
    <div className="w-full space-y-6 pb-8 animate-in fade-in duration-300">
      {/* Header & Title */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3.5 sm:gap-4">
        <div className="flex items-start sm:items-center gap-3.5 min-w-0 flex-1">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("courseDetails.backBtn")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0 mt-0.5 sm:mt-0"
          />
          <div className="flex flex-col justify-center min-w-0 flex-1">
            {selectedCourse.isCez && (
              <span className="text-xs font-bold text-primary uppercase tracking-wider self-start mb-1">
                {t("courses.tagCez")}
              </span>
            )}
            <h1 className="text-lg sm:text-xl font-bold text-foreground break-words leading-tight">
              {selectedCourse.name}
            </h1>
          </div>
        </div>

        {!selectedCourse.isCez && (
          <div className="flex items-center gap-2.5 w-full sm:w-auto shrink-0">
            <SecondaryButton
              type="button"
              onClick={() => setIsEditModalOpen(true)}
              icon={<Pencil size={15} strokeWidth={2.25} />}
              className="py-2.5 px-3.5 text-xs font-semibold flex-1 sm:flex-initial justify-center"
            >
              {t("quizDetails.editBtn")}
            </SecondaryButton>
            <SecondaryButton
              type="button"
              onClick={() => setIsDeleteModalOpen(true)}
              icon={<Trash2 size={15} strokeWidth={2.25} className="text-rose-400" />}
              className="py-2.5 px-3.5 text-xs font-semibold flex-1 sm:flex-initial justify-center"
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
                <div className="py-4 text-center">
                  <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                    {t("courseDetails.noFiles")}
                  </p>
                </div>
              ) : (
                <div className="space-y-2">
                  {courseFiles.map((file) => (
                    <div
                      key={file.id}
                      className={`flex items-center justify-between p-3.5 rounded-xl border transition-all ${
                        file.isHidden
                          ? "bg-muted/15 border-dashed border-border/60"
                          : "bg-muted/40 border-border"
                      }`}
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-8 h-8 rounded-lg bg-card border border-border flex items-center justify-center text-muted-foreground flex-shrink-0">
                          <FileText size={15} />
                        </div>
                        <div className="min-w-0">
                          <div className={`text-xs font-medium truncate ${file.isHidden ? "text-muted-foreground" : "text-foreground"}`}>
                            {file.displayName}
                          </div>
                          <div className="text-[11px] text-muted-foreground/60 flex items-center gap-1.5 flex-wrap">
                            <span>{file.mimeType}</span>
                            {file.estimatedTokens !== undefined && file.estimatedTokens > 0 && (
                              <>
                                <span>•</span>
                                <span className="font-medium">
                                  {t("courseDetails.tokensLabel", {
                                    count: file.estimatedTokens.toLocaleString(),
                                    pct: file.estimatedDailyUsagePercentage ?? 0,
                                  })}
                                </span>
                              </>
                            )}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 shrink-0 ml-2">
                        <button
                          type="button"
                          onClick={() => handleToggleFileVisibility(file.id)}
                          title={file.isHidden ? t("courseDetails.unhideFile") : t("courseDetails.hideFile")}
                          aria-label={file.isHidden ? t("courseDetails.unhideFile") : t("courseDetails.hideFile")}
                          className="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
                        >
                          {file.isHidden ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                        <button
                          type="button"
                          onClick={() => handleFileDownload(file.id, file.displayName)}
                          title={t("courseDetails.downloadFile")}
                          aria-label={t("courseDetails.downloadFile")}
                          className="p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
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
                <div className="py-4 text-center">
                  <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                    {t("quizzes.noQuizzesShort", "Brak quizów")}
                  </p>
                </div>
              ) : (
                <div className="space-y-1.5 sm:space-y-2">
                  {courseQuizzes.map((quiz, idx) => (
                    <QuizCard
                      key={quiz.id}
                      quiz={quiz}
                      index={idx + 1}
                      showCourseName={false}
                      onDelete={() => setQuizToDelete({ id: quiz.id, name: quiz.name })}
                    />
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <AccordionHeader
          title={t("chat.title")}
          count={threads.length}
          isExpanded={isChatExpanded}
          onToggle={() => setIsChatExpanded(!isChatExpanded)}
          onAddClick={handleCreateChatThread}
          addTitle={t("chat.newThreadBtn", "Nowy wątek")}
        />

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isChatExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden p-1 -m-1">
            <div className="pt-3.5 border-t border-border mt-3.5 px-0.5">
              {threads.length === 0 ? (
                <div className="py-4 text-center">
                  <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                    {t("chat.noThreadsShort", "Brak czatów")}
                  </p>
                </div>
              ) : (
                <div className="space-y-1.5 sm:space-y-2">
                  {threads.map((thread, idx) => (
                    <ChatThreadCard
                      key={thread.id}
                      thread={thread}
                      index={idx + 1}
                      showCourseName={false}
                      onDelete={(e) => {
                        e.stopPropagation();
                        deleteThread(thread.id);
                      }}
                    />
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
                <div className="py-4 text-center">
                  <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                    {t("flashcards.noCourseDecksShort", "Brak fiszek")}
                  </p>
                </div>
              ) : (
                <div className="space-y-1.5 sm:space-y-2">
                  {decks.map((deck, idx) => (
                    <FlashcardDeckCard
                      key={deck.id}
                      deck={deck}
                      index={idx + 1}
                      showCourseName={false}
                      onDelete={() => setDeckToDelete({ id: deck.id, name: deck.name })}
                    />
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

      <ConfirmModal
        isOpen={!!quizToDelete}
        onClose={() => setQuizToDelete(null)}
        onConfirm={handleConfirmDeleteQuiz}
        title="Usuń quiz"
        message={`Czy na pewno chcesz usunąć quiz "${quizToDelete?.name || ""}"?`}
        confirmBtnText="Usuń quiz"
        isDestructive
      />

      <ConfirmModal
        isOpen={!!deckToDelete}
        onClose={() => setDeckToDelete(null)}
        onConfirm={handleConfirmDeleteDeck}
        title="Usuń talię fiszek"
        message={`Czy na pewno chcesz usunąć talię fiszek "${deckToDelete?.name || ""}"?`}
        confirmBtnText="Usuń talię"
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
        hasFiles={courseFiles.some((f) => !f.isHidden)}
        courseId={id || ""}
      />

      <GenerateFlashcardsModal
        isOpen={isGenerateFlashcardsModalOpen}
        onClose={() => setIsGenerateFlashcardsModalOpen(false)}
        onSubmit={handleGenerateFlashcardsSubmit}
        hasFiles={courseFiles.some((f) => !f.isHidden)}
        courseId={id || ""}
      />
    </div>
  );
}
