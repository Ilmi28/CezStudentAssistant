import { useState, useEffect } from "react";
import Modal from "./Modal";
import { PrimaryButton, SecondaryButton } from "./Button";
import DifficultyControlsGroup from "./DifficultyControlsGroup";
import type { FlashcardDto } from "../types/flashcardTypes";
import { QuestionDifficulty } from "../enums/quizEnums";

interface StartFlashcardStudyModalProps {
  isOpen: boolean;
  onClose: () => void;
  onStart: (easyCount: number, mediumCount: number, hardCount: number) => void;
  cards: FlashcardDto[];
}

export function StartFlashcardStudyModal({
  isOpen,
  onClose,
  onStart,
  cards,
}: StartFlashcardStudyModalProps) {
  const isEasy = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Easy || diff === 1 || diff === "Easy" || diff === "1";
  const isHard = (diff?: QuestionDifficulty | string | number) =>
    diff === QuestionDifficulty.Hard || diff === 3 || diff === "Hard" || diff === "3";

  const easyInDeck = cards.filter((c) => isEasy(c.difficulty)).length;
  const hardInDeck = cards.filter((c) => isHard(c.difficulty)).length;
  const mediumInDeck = cards.filter((c) => !isEasy(c.difficulty) && !isHard(c.difficulty)).length;

  const [easyCount, setEasyCount] = useState(easyInDeck);
  const [mediumCount, setMediumCount] = useState(mediumInDeck);
  const [hardCount, setHardCount] = useState(hardInDeck);

  useEffect(() => {
    if (isOpen) {
      setEasyCount(easyInDeck);
      setMediumCount(mediumInDeck);
      setHardCount(hardInDeck);
    }
  }, [isOpen, easyInDeck, mediumInDeck, hardInDeck]);

  const totalSelected = easyCount + mediumCount + hardCount;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (totalSelected === 0) return;
    onStart(easyCount, mediumCount, hardCount);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Rozpocznij naukę fiszek" maxWidth="md">
      <form onSubmit={handleSubmit} className="space-y-5">
        <p className="text-xs text-muted-foreground">
          Wybierz liczbę fiszek poszczególnych poziomów trudności, które mają wejść w skład tej sesji nauki.
        </p>

        <DifficultyControlsGroup
          easyCount={easyCount}
          mediumCount={mediumCount}
          hardCount={hardCount}
          setEasyCount={setEasyCount}
          setMediumCount={setMediumCount}
          setHardCount={setHardCount}
          totalSelected={totalSelected}
          itemUnitLabel="fiszek"
          maxPerCategory={Math.max(easyInDeck, mediumInDeck, hardInDeck, 100)}
        />

        <div className="flex justify-end gap-3 pt-4 border-t border-border">
          <SecondaryButton type="button" onClick={onClose}>
            Anuluj
          </SecondaryButton>
          <PrimaryButton type="submit" disabled={totalSelected === 0}>
            Rozpocznij naukę ({totalSelected})
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
