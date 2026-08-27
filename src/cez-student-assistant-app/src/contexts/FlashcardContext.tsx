import { createContext, useState, type ReactNode } from "react";
import type { FlashcardDeckDto } from "../types/flashcardTypes";

export interface FlashcardContextState {
  decks: FlashcardDeckDto[];
  setDecks: React.Dispatch<React.SetStateAction<FlashcardDeckDto[]>>;
}

export const FlashcardContext = createContext<FlashcardContextState | null>(null);

export function FlashcardProvider({ children }: { children: ReactNode }) {
  const [decks, setDecks] = useState<FlashcardDeckDto[]>([]);

  return (
    <FlashcardContext.Provider value={{ decks, setDecks }}>
      {children}
    </FlashcardContext.Provider>
  );
}
