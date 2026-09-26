import React, { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Paperclip, Send, Loader2 } from "lucide-react";
import Card from "../../ui/Card";
import { Input } from "../../ui/Input";
import { PrimaryButton, SecondaryButton } from "../../ui/Button";
import { Alert } from "../../ui/Alert";
import { ChatMessageBubble } from "./ChatMessageBubble";
import { AttachedResourcesModal } from "../../modals/AttachedResourcesModal";
import { TokenUsagePopover } from "./TokenUsagePopover";
import { useUser } from "../../../hooks";
import type { ChatThreadDto, ChatMessageDto, CourseResourceDto, UserUsageDto } from "../../../types";

interface CourseChatWindowProps {
  activeThread: ChatThreadDto | null;
  messages: ChatMessageDto[];
  loadingMessages: boolean;
  streaming: boolean;
  error: string | null;
  resources?: CourseResourceDto[];
  onSendMessage: (text: string) => void;
  onUpdateResources?: (resourceIds: string[]) => void;
}

interface ChatInputFormProps {
  streaming: boolean;
  hasResources: boolean;
  usage?: UserUsageDto | null;
  onSendMessage: (text: string) => void;
  onOpenModal: () => void;
}

const ChatInputForm = React.memo(function ChatInputForm({
  streaming,
  hasResources,
  usage,
  onSendMessage,
  onOpenModal,
}: ChatInputFormProps) {
  const { t } = useTranslation();
  const [inputText, setInputText] = useState("");

  const handleSend = (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputText.trim() || streaming) return;
    onSendMessage(inputText);
    setInputText("");
  };

  return (
    <div className="shrink-0 sticky bottom-0 bg-card border-t border-border p-3 md:p-4">
      <form onSubmit={handleSend} className="flex items-center gap-2.5">
        {hasResources && (
          <SecondaryButton
            type="button"
            onClick={onOpenModal}
            aria-label="Materiały AI"
            title="Materiały AI"
            icon={<Paperclip size={18} />}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          />
        )}
        <div className="flex-1 relative flex items-center">
          <Input
            type="text"
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            disabled={streaming}
            className="w-full text-sm py-2.5 pr-10"
          />
          {usage && (
            <div className="absolute right-2 flex items-center pointer-events-auto z-10">
              <TokenUsagePopover usage={usage} />
            </div>
          )}
        </div>
        <PrimaryButton
          type="submit"
          disabled={!inputText.trim() || streaming}
          aria-label={streaming ? t("chat.thinking") : t("chat.sendBtn")}
          title={streaming ? t("chat.thinking") : t("chat.sendBtn")}
          icon={streaming ? <Loader2 size={18} className="animate-spin" /> : <Send size={18} />}
          className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
        />
      </form>
    </div>
  );
});

export const CourseChatWindow: React.FC<CourseChatWindowProps> = ({
  activeThread,
  messages,
  loadingMessages,
  streaming,
  error,
  resources = [],
  onSendMessage,
  onUpdateResources,
}) => {
  const { t } = useTranslation();
  const { usage, fetchUserUsage } = useUser();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);
  const isUserScrolledUpRef = useRef<boolean>(false);

  const handleScroll = () => {
    if (!chatContainerRef.current) return;
    const { scrollTop, scrollHeight, clientHeight } = chatContainerRef.current;
    const isAtBottom = scrollHeight - scrollTop - clientHeight <= 80;
    isUserScrolledUpRef.current = !isAtBottom;
  };

  const scrollToBottom = () => {
    if (!isUserScrolledUpRef.current) {
      messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, streaming]);

  useEffect(() => {
    if (!streaming) {
      fetchUserUsage();
    }
  }, [streaming, fetchUserUsage]);

  const handleSendMessage = React.useCallback(
    (text: string) => {
      isUserScrolledUpRef.current = false;
      onSendMessage(text);
    },
    [onSendMessage]
  );

  if (!activeThread) {
    return (
      <Card className="flex flex-col items-center justify-center h-full p-8 border border-border bg-card text-center">
        <div className="text-4xl mb-3">💬</div>
        <h3 className="text-base font-bold text-foreground mb-1">{t("chat.noThreads")}</h3>
        <p className="text-xs text-muted-foreground max-w-sm">{t("chat.noThreadsSubtitle")}</p>
      </Card>
    );
  }

  const toggleResource = (resourceId: string) => {
    if (!onUpdateResources) return;
    const current = activeThread.attachedResourceIds || [];
    const updated = current.includes(resourceId)
      ? current.filter((id) => id !== resourceId)
      : [...current, resourceId];
    onUpdateResources(updated);
  };

  const handleSelectAllResources = () => {
    if (!onUpdateResources) return;
    const visibleIds = resources.filter((r) => !r.isHidden).map((r) => r.id);
    const targetIds = visibleIds.length > 0 ? visibleIds : resources.map((r) => r.id);
    onUpdateResources(targetIds);
  };

  const handleDeselectAllResources = () => {
    if (!onUpdateResources) return;
    onUpdateResources([]);
  };

  const attachedResourceIds = activeThread.attachedResourceIds || [];

  return (
    <Card className="flex flex-col flex-1 h-full border-y sm:border border-border bg-card overflow-hidden -mx-4 sm:mx-0 rounded-none sm:rounded-2xl">
      {error && (
        <div className="p-3 shrink-0">
          <Alert variant="error" message={error} />
        </div>
      )}

      <div
        ref={chatContainerRef}
        onScroll={handleScroll}
        className="flex-1 overflow-y-auto p-3 sm:p-4 md:p-6 space-y-3 sm:space-y-4"
      >
        {loadingMessages ? (
          <div className="flex items-center justify-center py-8 text-xs text-muted-foreground">
            {t("common.loading")}
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-center text-muted-foreground">
            <p className="text-xs">Rozpocznij dyskusję wpisując wiadomość poniżej.</p>
          </div>
        ) : (
          messages.map((msg) => <ChatMessageBubble key={msg.id} message={msg} />)
        )}
        <div ref={messagesEndRef} />
      </div>

      <ChatInputForm
        streaming={streaming}
        hasResources={resources.length > 0}
        usage={usage}
        onSendMessage={handleSendMessage}
        onOpenModal={() => setIsModalOpen(true)}
      />

      <AttachedResourcesModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        resources={resources}
        attachedResourceIds={attachedResourceIds}
        onToggleResource={toggleResource}
        onSelectAll={handleSelectAllResources}
        onDeselectAll={handleDeselectAllResources}
      />
    </Card>
  );
};

export default CourseChatWindow;
