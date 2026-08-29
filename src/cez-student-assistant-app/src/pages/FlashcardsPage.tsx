import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { BookOpen } from "lucide-react";
import Card from "../components/Card";
import { FlashcardDeckCard } from "../components/FlashcardDeckCard";
import { useFlashcards } from "../hooks";

export default function FlashcardsPage() {
  const { t } = useTranslation();
  const { decks, refreshDecks } = useFlashcards();

  useEffect(() => {
    refreshDecks();
  }, []);

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <div className="flex items-center justify-between mb-4 border-b border-border pb-2">
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
            {t("nav.flashcards")}
          </h3>
        </div>

        {decks.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <BookOpen size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <p className="text-sm text-muted-foreground">{t("flashcards.noDecks")}</p>
              <p className="text-xs text-muted-foreground/60 mt-1">{t("flashcards.noDecksSubtitle")}</p>
            </div>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {decks.map((deck, idx) => (
              <FlashcardDeckCard key={deck.id} deck={deck} index={idx + 1} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
