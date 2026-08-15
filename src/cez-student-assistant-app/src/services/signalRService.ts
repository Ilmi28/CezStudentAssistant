import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";
import { API_BASE_URL } from "./baseClient";

class SignalRService {
  private connection: HubConnection | null = null;
  private listeners: ((jobId: string, status: string) => void)[] = [];

  public startConnection(): HubConnection {
    if (this.connection) {
      return this.connection;
    }

    const hubUrl = `${API_BASE_URL}/sync-hub`;

    this.connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on("CezSyncStatusUpdated", (jobId: string, status: string) => {
      this.listeners.forEach((listener) => listener(jobId, status));
    });

    this.connection.start().catch((err) => {
      console.debug("[SignalR] Connection suppressed/failed:", err);
    });

    return this.connection;
  }

  public subscribeJobStatus(callback: (jobId: string, status: string) => void): () => void {
    this.listeners.push(callback);
    return () => {
      this.listeners = this.listeners.filter((cb) => cb !== callback);
    };
  }

  public stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
    }
  }
}

export const signalRService = new SignalRService();
