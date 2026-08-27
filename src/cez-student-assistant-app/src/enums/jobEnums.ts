export enum JobStatus {
  Enqueued = 1,
  Processing = 2,
  Succeeded = 3,
  Failed = 4,
}

export enum JobType {
  CezSync = 1,
  QuizGeneration = 2,
  FlashcardGeneration = 3,
}
