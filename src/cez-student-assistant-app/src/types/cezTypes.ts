export interface CezStatusDto {
  isConnected: boolean;
  lastSyncAt: string | null;
  lastSyncStatus: number | null;
}

export interface CezSyncResponseDto {
  jobId?: string;
  message?: string;
}
