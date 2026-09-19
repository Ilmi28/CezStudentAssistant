import React from "react";
import { useTranslation } from "react-i18next";
import Card from "../../ui/Card";
import { PrimaryButton } from "../../ui/Button";
import type { ChatThreadDto } from "../../../types";

interface CourseChatThreadListProps {
  threads: ChatThreadDto[];
  activeThreadId: string | null;
  onSelectThread: (id: string) => void;
  onCreateThread: () => void;
  onDeleteThread: (id: string) => void;
}

export const CourseChatThreadList: React.FC<CourseChatThreadListProps> = ({
  threads,
  activeThreadId,
  onSelectThread,
  onCreateThread,
  onDeleteThread,
}) => {
  const { t } = useTranslation();

  return (
    <Card className="flex flex-col h-full p-4 border border-border bg-card">
      <div className="flex items-center justify-between mb-4 pb-3 border-b border-border">
        <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
          {t("chat.title")}
        </h3>
        <PrimaryButton onClick={onCreateThread} className="text-xs px-2.5 py-1">
          + {t("chat.newThreadBtn")}
        </PrimaryButton>
      </div>

      {threads.length === 0 ? (
        <div className="flex flex-col items-center justify-center flex-1 py-8 text-center">
          <p className="text-xs text-muted-foreground">{t("chat.noThreads")}</p>
        </div>
      ) : (
        <div className="space-y-2 overflow-y-auto flex-1 pr-1">
          {threads.map((thread) => {
            const isActive = thread.id === activeThreadId;
            return (
              <div
                key={thread.id}
                onClick={() => onSelectThread(thread.id)}
                className={`group flex items-center justify-between p-3 rounded-lg border transition-all cursor-pointer ${
                  isActive
                    ? "bg-sidebar border-primary/60 text-foreground font-medium shadow-sm"
                    : "bg-background/50 border-border text-muted-foreground hover:border-primary/45 hover:text-foreground"
                }`}
              >
                <div className="flex-1 min-w-0 pr-2">
                  <div className="text-xs font-semibold truncate text-foreground">
                    {thread.title}
                  </div>
                  {thread.lastMessageSnippet && (
                    <div className="text-[11px] truncate text-muted-foreground/70 mt-0.5">
                      {thread.lastMessageSnippet}
                    </div>
                  )}
                </div>

                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (window.confirm(t("chat.deleteThreadConfirm"))) {
                      onDeleteThread(thread.id);
                    }
                  }}
                  className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-red-400 p-1 text-xs transition-opacity"
                  title={t("common.cancel")}
                >
                  ✕
                </button>
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
};
