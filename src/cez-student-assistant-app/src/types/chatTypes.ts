import { ChatRoleEnum } from "../enums";

export interface ChatThreadDto {
  id: string;
  courseId: string;
  courseName: string;
  userId: string;
  title: string;
  createdAt: string;
  lastMessageAt?: string;
  lastMessageSnippet?: string;
  attachedResourceIds?: string[];
}

export interface ChatMessageDto {
  id: string;
  chatThreadId: string;
  role: ChatRoleEnum | string;
  content: string;
  tokenCount: number;
  createdAt: string;
}

export interface CreateChatThreadRequest {
  courseId: string;
  title?: string;
  attachedResourceIds?: string[];
}

export interface SendChatMessageStreamRequest {
  userMessage: string;
}

export interface UpdateChatThreadResourcesRequest {
  resourceIds: string[];
}
