import React from "react";
import { useCourseChat } from "../../../hooks";
import { CourseChatThreadList } from "./CourseChatThreadList";
import { CourseChatWindow } from "./CourseChatWindow";
import type { CourseResourceDto } from "../../../types";

interface CourseChatPanelProps {
  courseId: string;
  resources?: CourseResourceDto[];
}

export const CourseChatPanel: React.FC<CourseChatPanelProps> = ({ courseId, resources }) => {
  const {
    threads,
    activeThreadId,
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

  const handleCreateThread = async () => {
    await createThread({});
  };

  return (
    <div className="flex flex-col lg:flex-row gap-4 h-[650px] w-full">
      <div className="w-full lg:w-1/3 h-full">
        <CourseChatThreadList
          threads={threads}
          activeThreadId={activeThreadId}
          onSelectThread={selectThread}
          onCreateThread={handleCreateThread}
          onDeleteThread={deleteThread}
        />
      </div>

      <div className="w-full lg:w-2/3 h-full">
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
  );
};
