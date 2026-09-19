import { API_BASE_URL, customFetch, handleResponse } from "./baseClient";
import { authService } from "./authService";
import type {
  ChatThreadDto,
  ChatMessageDto,
  CreateChatThreadRequest,
  SendChatMessageStreamRequest,
  PagedResultDto,
  PagedQueryParams,
} from "../types";

export const chatService = {
  async getCourseChatThreads(
    courseId?: string,
    params?: PagedQueryParams
  ): Promise<PagedResultDto<ChatThreadDto>> {
    const urlParams = new URLSearchParams();
    if (courseId) urlParams.append("courseId", courseId);
    if (params?.pageNumber) urlParams.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) urlParams.append("pageSize", params.pageSize.toString());
    if (params?.searchTerm) urlParams.append("searchTerm", params.searchTerm);

    const queryString = urlParams.toString() ? `?${urlParams.toString()}` : "";
    const res = await customFetch(
      `${API_BASE_URL}/chats${queryString}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<PagedResultDto<ChatThreadDto>>(res);
  },

  async getChatThreadDetails(threadId: string): Promise<ChatThreadDto> {
    const res = await customFetch(
      `${API_BASE_URL}/chats/${threadId}`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<ChatThreadDto>(res);
  },

  async createChatThread(request: CreateChatThreadRequest): Promise<ChatThreadDto> {
    const res = await customFetch(
      `${API_BASE_URL}/chats`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<ChatThreadDto>(res);
  },

  async getChatThreadMessages(threadId: string): Promise<ChatMessageDto[]> {
    const res = await customFetch(
      `${API_BASE_URL}/chats/${threadId}/messages`,
      { method: "GET" },
      false,
      authService.refreshToken
    );
    return handleResponse<ChatMessageDto[]>(res);
  },

  async deleteChatThread(threadId: string): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/chats/${threadId}`,
      { method: "DELETE" },
      false,
      authService.refreshToken
    );
    return handleResponse<void>(res);
  },

  async updateThreadResources(threadId: string, resourceIds: string[]): Promise<ChatThreadDto> {
    const res = await customFetch(
      `${API_BASE_URL}/chats/${threadId}/resources`,
      {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ resourceIds }),
      },
      false,
      authService.refreshToken
    );
    return handleResponse<ChatThreadDto>(res);
  },

  async streamChatMessage(
    threadId: string,
    request: SendChatMessageStreamRequest,
    onChunk: (chunk: string) => void
  ): Promise<void> {
    const res = await customFetch(
      `${API_BASE_URL}/chats/${threadId}/stream`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      },
      false,
      authService.refreshToken
    );

    if (!res.ok) {
      const errorText = await res.text();
      try {
        const parsed = JSON.parse(errorText);
        throw new Error(parsed.message || parsed.title || "Failed to stream chat response");
      } catch {
        throw new Error(errorText || `HTTP ${res.status}`);
      }
    }

    const reader = res.body?.getReader();
    if (!reader) {
      throw new Error("No readable stream response body");
    }

    const decoder = new TextDecoder();
    let buffer = "";

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split("\n\n");
      buffer = lines.pop() || "";

      for (const line of lines) {
        const trimmed = line.trim();
        if (trimmed.startsWith("data: ")) {
          const dataContent = trimmed.slice(6).trim();
          if (dataContent === "[DONE]") {
            return;
          }
          try {
            const parsed = JSON.parse(dataContent);
            if (parsed.chunk) {
              onChunk(parsed.chunk);
            }
          } catch {
            if (dataContent) {
              onChunk(dataContent);
            }
          }
        }
      }
    }
  },
};
