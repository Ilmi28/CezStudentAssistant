import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { BookOpen } from "lucide-react";
import { Card, FlashcardDeckCard, Flex, Grid, Heading, Text } from "../components";
import { useFlashcards } from "../hooks";
import { signalRService } from "../services/signalRService";

export default function FlashcardsPage() {
  const { t } = useTranslation();
  const { decks, refreshDecks } = useFlashcards();

  useEffect(() => {
    refreshDecks();

    signalRService.startConnection();
    const unsubscribe = signalRService.subscribeJobStatus((_jobId, status) => {
      if (status === "Succeeded" || status === "Failed") {
        refreshDecks();
      }
    });

    return () => {
      unsubscribe();
    };
  }, []);

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <div>
        <Flex align="center" justify="between" className="mb-4 border-b border-border pb-2">
          <Heading level={3} size="sm" uppercase className="tracking-wider">
            {t("nav.flashcards")}
          </Heading>
        </Flex>

        {decks.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <BookOpen size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <Text size="sm" variant="muted">{t("flashcards.noDecks")}</Text>
              <Text size="xs" variant="subtle" className="mt-1">{t("flashcards.noDecksSubtitle")}</Text>
            </div>
          </Card>
        ) : (
          <Grid cols={1} mdCols={2} gap={4}>
            {decks.map((deck, idx) => (
              <FlashcardDeckCard key={deck.id} deck={deck} index={idx + 1} />
            ))}
          </Grid>
        )}
      </div>
    </div>
  );
}
