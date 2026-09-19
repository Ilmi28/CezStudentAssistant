import { memo } from "react";
import type { ChatMessageDto } from "../../../types";
import { ChatRoleEnum } from "../../../enums";
import { MarkdownRenderer } from "../../ui/MarkdownRenderer";
import { formatTime } from "../../../helpers/dateHelper";

interface ChatMessageBubbleProps {
  message: ChatMessageDto;
}

export const ChatMessageBubble = memo(function ChatMessageBubble({ message }: ChatMessageBubbleProps) {
  const isUser = String(message.role).toLowerCase() === ChatRoleEnum.User;

  const formattedTime = message.createdAt
    ? formatTime(message.createdAt, {
        hour: "2-digit",
        minute: "2-digit",
      })
    : null;

  return (
    <div className={`flex flex-col w-full mb-3 ${isUser ? "items-end" : "items-start"}`}>
      <div
        className={`max-w-[85%] md:max-w-[80%] rounded-2xl px-3.5 py-2 border shadow-2xs ${
          isUser
            ? "bg-primary text-white border-primary"
            : "bg-card text-foreground border-border"
        }`}
      >
        {message.content ? (
          <MarkdownRenderer content={message.content} isUser={isUser} />
        ) : (
          <div className="flex items-center gap-2 py-1.5 px-1">
            <span className="w-2.5 h-2.5 rounded-full bg-primary animate-typing-dot" style={{ animationDelay: "0ms" }} />
            <span className="w-2.5 h-2.5 rounded-full bg-primary animate-typing-dot" style={{ animationDelay: "180ms" }} />
            <span className="w-2.5 h-2.5 rounded-full bg-primary animate-typing-dot" style={{ animationDelay: "360ms" }} />
          </div>
        )}
      </div>
      {formattedTime && (
        <span className="text-[10px] text-muted-foreground/60 tabular-nums mt-1 px-1 select-none">
          {formattedTime}
        </span>
      )}
    </div>
  );
});

export default ChatMessageBubble;
