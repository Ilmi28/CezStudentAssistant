import { useNavigate, useLocation } from "react-router-dom";
import type { MouseEvent } from "react";
import { useTranslation } from "react-i18next";
import { Trash2 } from "lucide-react";
import type { ChatThreadDto } from "../../../types/chatTypes";
import { Card, Heading, Text, Flex } from "../../index";
import { formatDateTime } from "../../../helpers/dateHelper";

interface ChatThreadCardProps {
  thread: ChatThreadDto;
  index?: number;
  showCourseName?: boolean;
  className?: string;
  onClick?: () => void;
  onDelete?: (e: MouseEvent) => void;
}

export function ChatThreadCard({
  thread,
  index,
  showCourseName = true,
  className = "",
  onClick,
  onDelete,
}: ChatThreadCardProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const handleClick = () => {
    if (onClick) {
      onClick();
    } else {
      navigate(`/chats/${thread.id}`, { state: { fromPath: location.pathname } });
    }
  };

  const rawTitle = thread.title;
  const isGenericTitle = !rawTitle || rawTitle.startsWith("Wątek z") || rawTitle === thread.courseName;
  const displayTitle =
    index !== undefined && isGenericTitle
      ? `Wątek #${index}`
      : rawTitle || (index !== undefined ? `Wątek #${index}` : "Wątek czatu");

  const formattedDate = thread.createdAt
    ? formatDateTime(thread.createdAt, {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : null;

  return (
    <Card
      hoverEffect
      onClick={handleClick}
      className={`p-3.5 px-4 flex-row items-center justify-between gap-4 ${className}`}
    >
      <div className="min-w-0 flex-1 space-y-0.5">
        <Flex align="center" gap={2} wrap className="min-w-0">
          <Heading
            level={4}
            size="sm"
            className="leading-snug line-clamp-1 truncate tile-title-scale"
            title={displayTitle}
          >
            {displayTitle}
          </Heading>
        </Flex>
        {showCourseName && thread.courseName && (
          <Text size="xs" variant="muted" className="line-clamp-1 truncate">
            {thread.courseName}
          </Text>
        )}
        {thread.lastMessageSnippet && (
          <Text size="xs" variant="subtle" className="line-clamp-1 truncate italic">
            "{thread.lastMessageSnippet}"
          </Text>
        )}
      </div>

      <Flex align="center" gap={3} className="shrink-0">
        {formattedDate && (
          <Text size="xs" variant="subtle" className="tabular-nums">
            {formattedDate}
          </Text>
        )}

        {onDelete && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(e);
            }}
            title={t("common.delete", "Usuń")}
            aria-label={t("common.delete", "Usuń")}
            className="p-1.5 rounded-lg text-muted-foreground hover:text-destructive hover:bg-muted transition-colors cursor-pointer"
          >
            <Trash2 size={16} />
          </button>
        )}
      </Flex>
    </Card>
  );
}

export default ChatThreadCard;
