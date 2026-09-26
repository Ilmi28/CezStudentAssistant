import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, Trash2 } from "lucide-react";
import { chatService, courseService, type CourseResourceDto } from "../services";
import type { ChatThreadDto, ChatMessageDto } from "../types";
import {
  Badge,
  SecondaryButton,
  LoadingScreen,
  Alert,
  CourseChatWindow,
  ConfirmModal,
} from "../components";
import { useCourse } from "../hooks";

export default function ChatThreadDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const { courses } = useCourse();

  const isNew = id === "new";

  const [thread, setThread] = useState<ChatThreadDto | null>(null);
  const [courseFiles, setCourseFiles] = useState<CourseResourceDto[]>([]);
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMessages] = useState(false);
  const [streaming, setStreaming] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const searchParams = new URLSearchParams(location.search);
  const courseIdParam = searchParams.get("courseId");

  const fromPath =
    (location.state as { fromPath?: string })?.fromPath ||
    (thread?.courseId ? `/course/${thread.courseId}` : "/chats");

  const loadExistingThread = useCallback(async (threadId: string) => {
    setLoading(true);
    setErrorMsg(null);
    try {
      const data = await chatService.getChatThreadDetails(threadId);
      if (data.courseId) {
        const [files, msgs] = await Promise.all([
          courseService.getCourseFiles(data.courseId),
          chatService.getChatThreadMessages(threadId),
        ]);
        setCourseFiles(files);
        setMessages(msgs);
        if (!data.attachedResourceIds || data.attachedResourceIds.length === 0) {
          data.attachedResourceIds = files.filter((f) => !f.isHidden).map((f) => f.id);
        }
      }
      setThread(data);
    } catch (err: unknown) {
      console.warn("[ChatThreadDetailsPage] Failed to fetch thread:", err);
      setErrorMsg("Nie udało się pobrać szczegółów wątku czatu.");
    } finally {
      setLoading(false);
    }
  }, []);

  const initNewThread = useCallback(async () => {
    setLoading(true);
    setErrorMsg(null);
    try {
      let targetCourseId = courseIdParam;
      let defaultAttachedIds: string[] = [];

      if (!targetCourseId && courses.length > 0) {
        targetCourseId = courses[0].id;
      }

      if (targetCourseId) {
        const files = await courseService.getCourseFiles(targetCourseId);
        setCourseFiles(files);
        defaultAttachedIds = files.filter((f) => !f.isHidden).map((f) => f.id);
      }

      if (!targetCourseId) {
        setErrorMsg("Brak dostępnych przedmiotów do utworzenia czatu.");
        return;
      }

      const created = await chatService.createChatThread({
        courseId: targetCourseId,
        attachedResourceIds: defaultAttachedIds,
      });

      navigate(`/chats/${created.id}`, { replace: true, state: location.state });
    } catch (err: unknown) {
      console.warn("[ChatThreadDetailsPage] Failed to initialize new thread:", err);
      setErrorMsg("Nie udało się utworzyć nowego czatu.");
    } finally {
      setLoading(false);
    }
  }, [courseIdParam, courses, location.state, navigate]);

  useEffect(() => {
    if (!id) return;
    if (isNew) {
      initNewThread();
    } else if (thread?.id !== id) {
      loadExistingThread(id);
    }
  }, [id, isNew, initNewThread, loadExistingThread, thread?.id]);

  const handleGoBack = () => {
    navigate(fromPath);
  };

  const handleDeleteConfirm = async () => {
    if (!id || isNew) return;
    try {
      await chatService.deleteChatThread(id);
      navigate(fromPath);
    } catch (err: unknown) {
      console.warn("[ChatThreadDetailsPage] Failed to delete thread:", err);
    } finally {
      setIsDeleteModalOpen(false);
    }
  };

  const handleUpdateResources = async (resourceIds: string[]) => {
    if (!thread) return;
    try {
      const updated = await chatService.updateThreadResources(thread.id, resourceIds);
      setThread(updated);
    } catch (err: unknown) {
      console.warn("[ChatThreadDetailsPage] Failed to update thread resources:", err);
    }
  };

  const handleSendMessage = async (text: string) => {
    if (!thread || !text.trim() || streaming) return;

    const userMsgId = `temp-user-${Date.now()}`;
    const assistantMsgId = `temp-assistant-${Date.now()}`;

    const userMsg: ChatMessageDto = {
      id: userMsgId,
      chatThreadId: thread.id,
      role: "user",
      content: text.trim(),
      tokenCount: 0,
      createdAt: new Date().toISOString(),
    };

    const assistantMsg: ChatMessageDto = {
      id: assistantMsgId,
      chatThreadId: thread.id,
      role: "assistant",
      content: "",
      tokenCount: 0,
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMsg, assistantMsg]);
    setStreaming(true);
    setErrorMsg(null);

    try {
      await chatService.streamChatMessage(
        thread.id,
        { userMessage: text },
        (chunk: string) => {
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === assistantMsgId
                ? { ...msg, content: msg.content + chunk }
                : msg
            )
          );
        }
      );

      const refreshedThread = await chatService.getChatThreadDetails(thread.id);
      setThread(refreshedThread);
    } catch (err: unknown) {
      console.warn("[ChatThreadDetailsPage] Error during streaming:", err);
      setErrorMsg("Błąd podczas przesyłania odpowiedzi AI.");
      setMessages((prev) =>
        prev.map((msg) =>
          msg.id === assistantMsgId && !msg.content
            ? { ...msg, content: "Wystąpił błąd podczas uzyskiwania odpowiedzi od AI. Spróbuj ponownie." }
            : msg
        )
      );
    } finally {
      setStreaming(false);
    }
  };

  if (loading) {
    return <LoadingScreen message={t("common.loading", "Ładowanie czatu...")} />;
  }

  if (errorMsg && !thread) {
    return (
      <div className="flex-1 p-6 md:p-8 w-full space-y-4">
        <SecondaryButton onClick={handleGoBack} icon={<ChevronLeft size={16} />} size="sm">
          {t("common.back", "Wróć")}
        </SecondaryButton>
        <Alert variant="error" message={errorMsg} />
      </div>
    );
  }

  if (!thread) {
    return null;
  }

  return (
    <div className="w-full flex-1 min-h-0 flex flex-col gap-3.5 animate-in fade-in duration-300">
      <div className="flex items-center justify-between gap-3 shrink-0 px-1 sm:px-0">
        <div className="flex items-center gap-3 min-w-0 flex-1">
          <SecondaryButton
            type="button"
            onClick={handleGoBack}
            aria-label={t("common.back", "Wróć")}
            icon={<ChevronLeft size={22} strokeWidth={2.25} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
          <div className="flex flex-col justify-center min-w-0 flex-1">
            {thread.courseName && (
              <Badge variant="secondary" className="self-start mb-0.5">
                {thread.courseName}
              </Badge>
            )}
            <h1 className="text-base sm:text-xl font-bold text-foreground truncate leading-tight">
              {thread.title || "Nowy wątek czatu"}
            </h1>
          </div>
        </div>

        {!isNew && (
          <SecondaryButton
            type="button"
            onClick={() => setIsDeleteModalOpen(true)}
            aria-label={t("common.delete", "Usuń wątek")}
            title={t("common.delete", "Usuń wątek")}
            icon={<Trash2 size={18} strokeWidth={2} className="text-rose-500 group-hover:text-rose-400" />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0 hover:border-rose-500/40 hover:bg-rose-500/10"
          />
        )}
      </div>

      <div className="flex-1 min-h-0 w-full flex flex-col">
        <CourseChatWindow
          activeThread={thread}
          messages={messages}
          loadingMessages={loadingMessages}
          streaming={streaming}
          error={errorMsg}
          resources={courseFiles}
          onSendMessage={handleSendMessage}
          onUpdateResources={handleUpdateResources}
        />
      </div>

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        onConfirm={handleDeleteConfirm}
        title="Usuń wątek czatu"
        message="Czy na pewno chcesz usunąć ten wątek czatu wraz ze wszystkimi wiadomościami?"
        confirmBtnText="Usuń wątek"
        isDestructive
      />
    </div>
  );
}
