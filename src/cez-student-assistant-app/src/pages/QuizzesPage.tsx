import React, { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Brain, Search, X } from "lucide-react";
import { Card, QuizCard, Flex, Heading, Text, Input, Pagination, Badge } from "../components";
import { useQuiz } from "../hooks";

export default function QuizzesPage() {
  const { t } = useTranslation();
  const { quizzes, refreshQuizzes } = useQuiz();
  const [searchTerm, setSearchTerm] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const fetchQuizzes = async (page: number, search: string) => {
    const res = await refreshQuizzes({
      pageNumber: page,
      pageSize: 10,
      searchTerm: search,
    });
    if (res) {
      setTotalPages(res.totalPages);
      setTotalCount(res.totalCount);
    }
  };

  useEffect(() => {
    fetchQuizzes(pageNumber, searchTerm);
  }, [pageNumber, searchTerm]);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setPageNumber(1);
  };

  const handleClearSearch = () => {
    setSearchTerm("");
    setPageNumber(1);
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      <div>
        <Flex align="center" justify="between" className="mb-5 border-b border-border pb-4 flex-wrap gap-4">
          <Flex align="center" gap={3}>
            <Heading level={3} size="sm" uppercase className="tracking-wider">
              {t("quizzes.title")}
            </Heading>
            {totalCount > 0 && (
              <Badge variant="secondary" className="px-2.5 py-0.5 text-xs font-semibold">
                {totalCount}
              </Badge>
            )}
          </Flex>

          <div className="relative w-full sm:w-80">
            <Search size={16} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none" />
            <Input
              type="text"
              value={searchTerm}
              onChange={handleSearchChange}
              placeholder={t("quizzes.searchPlaceholder")}
              className="pl-9.5 pr-8 text-xs py-2 bg-card border-border shadow-2xs focus:border-primary"
            />
            {searchTerm && (
              <button
                type="button"
                onClick={handleClearSearch}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
                title="Wyszczyść wyszukiwanie"
              >
                <X size={14} />
              </button>
            )}
          </div>
        </Flex>

        {quizzes.length === 0 ? (
          <Card className="p-8 text-center shadow-sm">
            <div>
              <Brain size={32} className="mx-auto text-muted-foreground/30 mb-2" />
              <Text size="sm" variant="muted">{t("quizzes.noQuizzes")}</Text>
              <Text size="xs" variant="subtle" className="mt-1">{t("quizzes.noQuizzesSubtitle")}</Text>
            </div>
          </Card>
        ) : (
          <div className="space-y-2.5">
            {quizzes.map((q, idx) => (
              <QuizCard key={q.id} quiz={q} index={(pageNumber - 1) * 10 + idx + 1} />
            ))}
          </div>
        )}

        <Pagination
          pageNumber={pageNumber}
          totalPages={totalPages}
          onPageChange={setPageNumber}
          className="mt-6"
        />
      </div>
    </div>
  );
}
