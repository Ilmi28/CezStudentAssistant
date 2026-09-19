import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { MessageSquare, Plus, Trash2 } from "lucide-react";
import Modal from "../ui/Modal";
import { CourseChatWindow } from "../domain/chat/CourseChatWindow";
import { SecondaryButton } from "../ui/Button";
import { useCourseChat } from "../../hooks/useCourseChat";
import type { CourseResourceDto } from "../../types/courseTypes";

interface ChatThreadModalProps {
  isOpen: boolean;
  onClose: () => void;
  courseId: string;
  initialThreadId?: string | null;
  resources?: CourseResourceDto[];
}

export function ChatThreadModal({
  isOpen,
  onClose,
  courseId,
  initialThreadId,
  resources = [],
}: ChatThreadModalProps) {
  const { t } = useTranslation();
  const {
    threads,
    activeThread,
    messages,
    loadingMessages,
    streaming,
    error,
    selectThread,
    createThread,
    deleteThread,
    updateThreadResources,
    sendMessage,
  } = useCourseChat(courseId);

  useEffect(() => {
    if (isOpen && initialThreadId) {
      selectThread(initialThreadId);
    }
  }, [isOpen, initialThreadId, selectThread]);

  const handleCreateThread = async () => {
    await createThread({});
  };

  const handleDeleteActiveThread = async () => {
    if (!activeThread) return;
    await deleteThread(activeThread.id);
    if (threads.length <= 1) {
      onClose();
    }
  };

  const titleText = activeThread?.title || t("chat.title", "Czat AI");

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={titleText}
      maxWidth="4xl"
      icon={<MessageSquare className="text-primary" size={20} />}
    >
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-2 border-b border-border pb-3 flex-wrap">
          <div className="flex items-center gap-1.5 overflow-x-auto max-w-[70%] pb-1">
            {threads.map((thread, idx) => {
              const isSelected = thread.id === activeThread?.id;
              const displayTitle = thread.title || `Wątek #${idx + 1}`;
              return (
                <button
                  key={thread.id}
                  type="button"
                  onClick={() => selectThread(thread.id)}
                  className={`px-3 py-1 rounded-lg text-xs font-semibold border transition-all cursor-pointer whitespace-nowrap truncate max-w-[160px] ${
                    isSelected
                      ? "bg-primary text-white border-primary"
                      : "bg-muted/40 border-border text-muted-foreground hover:text-foreground hover:border-primary/45"
                  }`}
                  title={displayTitle}
                >
                  {displayTitle}
                </button>
              );
            })}
          </div>

          <div className="flex items-center gap-2">
            <SecondaryButton
              type="button"
              onClick={handleCreateThread}
              icon={<Plus size={15} strokeWidth={2.25} />}
              className="py-1.5 px-3 text-xs"
            >
              {t("chat.newThreadBtn", "Nowy wątek")}
            </SecondaryButton>

            {activeThread && (
              <SecondaryButton
                type="button"
                onClick={handleDeleteActiveThread}
                icon={<Trash2 size={15} strokeWidth={2.25} className="text-rose-400" />}
                className="py-1.5 px-3 text-xs"
                title={t("common.delete", "Usuń wątek")}
              />
            )}
          </div>
        </div>

        <div className="h-[520px] w-full">
          <CourseChatWindow
            activeThread={activeThread}
            messages={messages}
            loadingMessages={loadingMessages}
            streaming={streaming}
            error={error}
            resources={resources}
            onSendMessage={sendMessage}
            onUpdateResources={updateThreadResources}
          />
        </div>
      </div>
    </Modal>
  );
}

export default ChatThreadModal;
