import { useState, useCallback, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { chatService, courseService } from "../services";
import type { ChatThreadDto, ChatMessageDto, PagedQueryParams, PagedResultDto } from "../types";

export function useCourseChat(courseId?: string) {
  const { t } = useTranslation();
  const [threads, setThreads] = useState<ChatThreadDto[]>([]);
  const [activeThreadId, setActiveThreadId] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [loadingThreads, setLoadingThreads] = useState<boolean>(false);
  const [loadingMessages, setLoadingMessages] = useState<boolean>(false);
  const [streaming, setStreaming] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [totalCount, setTotalCount] = useState<number>(0);

  const activeThread = threads.find((t) => t.id === activeThreadId) || null;

  const fetchThreads = useCallback(
    async (params?: PagedQueryParams): Promise<PagedResultDto<ChatThreadDto> | null> => {
      setLoadingThreads(true);
      setError(null);
      try {
        const data = await chatService.getCourseChatThreads(courseId, params);
        setThreads(data.items);
        setTotalPages(data.totalPages);
        setTotalCount(data.totalCount);
        if (data.items.length > 0 && !activeThreadId) {
          setActiveThreadId(data.items[0].id);
        }
        return data;
      } catch (err: unknown) {
        console.warn("[useCourseChat] Failed to fetch threads:", err);
        setError(t("chat.fetchThreadsError"));
        return null;
      } finally {
        setLoadingThreads(false);
      }
    },
    [courseId, activeThreadId, t]
  );

  const fetchMessages = useCallback(async (threadId: string) => {
    if (!threadId) return;
    setLoadingMessages(true);
    setError(null);
    try {
      const data = await chatService.getChatThreadMessages(threadId);
      setMessages(data);
    } catch (err: unknown) {
      console.warn("[useCourseChat] Failed to fetch messages:", err);
      setError(t("chat.fetchMessagesError"));
    } finally {
      setLoadingMessages(false);
    }
  }, [t]);

  useEffect(() => {
    fetchThreads();
  }, [courseId]);

  useEffect(() => {
    if (activeThreadId) {
      fetchMessages(activeThreadId);
    } else {
      setMessages([]);
    }
  }, [activeThreadId]);

  const selectThread = useCallback((threadId: string) => {
    setActiveThreadId(threadId);
  }, []);

  const createThread = useCallback(
    async (request: { courseId?: string; title?: string; attachedResourceIds?: string[] }): Promise<ChatThreadDto | null> => {
      const targetCourseId = request.courseId || courseId;
      if (!targetCourseId) {
        setError(t("chat.selectCourseError"));
        return null;
      }
      setError(null);
      try {
        let attachedIds = request.attachedResourceIds;
        if (!attachedIds) {
          try {
            const files = await courseService.getCourseFiles(targetCourseId);
            attachedIds = files.filter((f) => !f.isHidden).map((f) => f.id);
          } catch (fileErr: unknown) {
            console.warn("[useCourseChat] Failed to fetch course files for thread default:", fileErr);
          }
        }

        const newThread = await chatService.createChatThread({
          ...request,
          courseId: targetCourseId,
          attachedResourceIds: attachedIds,
        });
        setThreads((prev) => [newThread, ...prev]);
        setActiveThreadId(newThread.id);
        return newThread;
      } catch (err: unknown) {
        console.warn("[useCourseChat] Failed to create thread:", err);
        setError(t("chat.createThreadError"));
        return null;
      }
    },
    [courseId, t]
  );

  const deleteThread = useCallback(
    async (threadId: string) => {
      setError(null);
      try {
        await chatService.deleteChatThread(threadId);
        setThreads((prev) => prev.filter((t) => t.id !== threadId));
        if (activeThreadId === threadId) {
          const remaining = threads.filter((t) => t.id !== threadId);
          setActiveThreadId(remaining.length > 0 ? remaining[0].id : null);
        }
      } catch (err: unknown) {
        console.warn("[useCourseChat] Failed to delete thread:", err);
        setError(t("chat.deleteThreadError"));
      }
    },
    [activeThreadId, threads, t]
  );

  const updateThreadResources = useCallback(
    async (resourceIds: string[]) => {
      if (!activeThreadId) return;
      setError(null);
      try {
        const updated = await chatService.updateThreadResources(activeThreadId, resourceIds);
        setThreads((prev) =>
          prev.map((t) => (t.id === activeThreadId ? updated : t))
        );
      } catch (err: unknown) {
        console.warn("[useCourseChat] Failed to update thread resources:", err);
        setError(t("chat.updateResourcesError"));
      }
    },
    [activeThreadId, t]
  );

  const sendMessage = useCallback(
    async (text: string) => {
      if (!activeThreadId || !text.trim() || streaming) return;

      const userMsgId = `temp-user-${Date.now()}`;
      const assistantMsgId = `temp-assistant-${Date.now()}`;

      const userMsg: ChatMessageDto = {
        id: userMsgId,
        chatThreadId: activeThreadId,
        role: "user",
        content: text.trim(),
        tokenCount: 0,
        createdAt: new Date().toISOString(),
      };

      const assistantMsg: ChatMessageDto = {
        id: assistantMsgId,
        chatThreadId: activeThreadId,
        role: "assistant",
        content: "",
        tokenCount: 0,
        createdAt: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, userMsg, assistantMsg]);
      setStreaming(true);
      setError(null);

      try {
        await chatService.streamChatMessage(
          activeThreadId,
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

        await fetchThreads();
      } catch (err: unknown) {
        console.warn("[useCourseChat] Error during streaming:", err);
        setMessages((prev) =>
          prev.map((msg) =>
            msg.id === assistantMsgId && !msg.content
              ? { ...msg, content: t("chat.streamError") }
              : msg
          )
        );
      } finally {
        setStreaming(false);
      }
    },
    [activeThreadId, streaming, fetchThreads, t]
  );

  return {
    threads,
    activeThreadId,
    activeThread,
    messages,
    loadingThreads,
    loadingMessages,
    streaming,
    error,
    totalPages,
    totalCount,
    selectThread,
    createThread,
    deleteThread,
    updateThreadResources,
    sendMessage,
    fetchThreads,
    refetchThreads: fetchThreads,
  };
}
