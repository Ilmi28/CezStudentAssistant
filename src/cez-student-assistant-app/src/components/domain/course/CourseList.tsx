import React from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Plus, Search, X } from "lucide-react";
import { Card, Badge, Heading, Text, Flex, SecondaryButton, Tooltip, Input, Pagination, EmptyState } from "../../index";
import type { CourseDto } from "../../../services";

interface CourseListProps {
  courses: CourseDto[];
  totalCount: number;
  pageNumber: number;
  totalPages: number;
  searchTerm: string;
  onSearchChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onClearSearch: () => void;
  onPageChange: (newPage: number) => void;
  onOpenAddModal?: () => void;
}

export default function CourseList({
  courses,
  totalCount,
  pageNumber,
  totalPages,
  searchTerm,
  onSearchChange,
  onClearSearch,
  onPageChange,
  onOpenAddModal,
}: CourseListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div className="space-y-3">
      <Flex align="center" justify="between" className="border-b border-border pb-3 mb-3 flex-wrap gap-4">
        <Flex align="center" gap={3}>
          <Heading level={3} size="sm" uppercase className="tracking-wider">
            {t("courses.title")}
          </Heading>
          {totalCount > 0 && (
            <Badge variant="secondary" className="px-2.5 py-0.5 text-xs font-semibold">
              {totalCount}
            </Badge>
          )}
        </Flex>

        <Flex align="center" gap={3} className="w-full sm:w-auto">
          <div className="relative w-full sm:w-72">
            <Search size={16} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none" />
            <Input
              type="text"
              value={searchTerm}
              onChange={onSearchChange}
              placeholder={t("courses.searchPlaceholder")}
              className="pl-9.5 pr-8 text-xs py-2 bg-card border-border shadow-2xs focus:border-primary"
            />
            {searchTerm && (
              <button
                type="button"
                onClick={onClearSearch}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
                title="Wyczyść wyszukiwanie"
              >
                <X size={14} />
              </button>
            )}
          </div>

          {onOpenAddModal && (
            <Tooltip content={t("courses.addCourseBtn")}>
              <SecondaryButton
                type="button"
                onClick={onOpenAddModal}
                aria-label={t("courses.addCourseBtn")}
                icon={<Plus size={20} strokeWidth={2.25} />}
                className="w-9 h-9 p-0 flex items-center justify-center shrink-0"
              />
            </Tooltip>
          )}
        </Flex>
      </Flex>

      <div>
        {courses.length === 0 ? (
          searchTerm ? (
            <EmptyState
              title="Brak wyników wyszukiwania"
              description={`Nie znaleziono przedmiotów pasujących do frazy "${searchTerm}".`}
              action={
                <SecondaryButton onClick={onClearSearch} size="sm">
                  Wyczyść wyszukiwanie
                </SecondaryButton>
              }
            />
          ) : (
            <EmptyState
              title="Brak przedmiotów"
              description="Nie masz jeszcze żadnych przedmiotów w systemie. Uruchom synchronizację z CEZ lub dodaj nowy przedmiot ręcznie."
              action={
                onOpenAddModal ? (
                  <SecondaryButton onClick={onOpenAddModal} size="sm" icon={<Plus size={16} />}>
                    Dodaj nowy przedmiot
                  </SecondaryButton>
                ) : undefined
              }
            />
          )
        ) : (
          <div className="space-y-2.5">
            {courses.map((c) => (
              <Card
                key={c.id}
                hoverEffect
                onClick={() => navigate(`/course/${c.id}`)}
                className="px-5 py-3.5 cursor-pointer flex-row items-center justify-between shadow-xs"
              >
                <div className="min-w-0 pr-4">
                  <Text size="sm" className="font-medium text-foreground line-clamp-2 break-words tile-title-scale" title={c.name}>
                    {c.name}
                  </Text>
                </div>
                {c.isCez && (
                  <span className="text-xs font-bold text-primary uppercase tracking-wider shrink-0">
                    {t("courses.tagCez")}
                  </span>
                )}
              </Card>
            ))}
          </div>
        )}

        <Pagination
          pageNumber={pageNumber}
          totalPages={totalPages}
          totalCount={totalCount}
          onPageChange={onPageChange}
          className="mt-4"
        />
      </div>
    </div>
  );
}
