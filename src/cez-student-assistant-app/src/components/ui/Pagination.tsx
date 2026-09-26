import React from "react";
import { useTranslation } from "react-i18next";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Flex } from "./LayoutPrimitives";
import { Text } from "./Typography";
import { PrimaryButton, SecondaryButton } from "./Button";

export interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  totalCount?: number;
  onPageChange: (newPage: number) => void;
  showLabels?: boolean;
  className?: string;
}

export const Pagination: React.FC<PaginationProps> = ({
  pageNumber,
  totalPages,
  totalCount,
  onPageChange,
  showLabels = false,
  className = "",
}) => {
  const { t } = useTranslation();

  if (totalCount !== undefined && totalCount === 0) {
    return null;
  }

  const safeTotalPages = Math.max(1, totalPages);
  const safePageNumber = Math.max(1, Math.min(pageNumber, safeTotalPages));

  const getVisiblePages = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (safeTotalPages <= maxVisible) {
      for (let i = 1; i <= safeTotalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (safePageNumber > 3) {
        pages.push("...");
      }
      const start = Math.max(2, safePageNumber - 1);
      const end = Math.min(safeTotalPages - 1, safePageNumber + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (safePageNumber < safeTotalPages - 2) {
        pages.push("...");
      }
      pages.push(safeTotalPages);
    }
    return pages;
  };

  return (
    <Flex
      align="center"
      justify="between"
      className={`flex-col sm:flex-row gap-3 pt-5 pb-6 sm:pb-8 border-t border-border ${className}`}
    >
      <Text size="xs" variant="muted" className="order-2 sm:order-1 font-medium">
        {t("common.paginationInfo", { current: safePageNumber, total: safeTotalPages })}
      </Text>

      <Flex align="center" gap={1.5} className="order-1 sm:order-2">
        <SecondaryButton
          type="button"
          size="sm"
          onClick={() => onPageChange(safePageNumber - 1)}
          disabled={safePageNumber <= 1}
          aria-label={t("common.previous")}
          title={t("common.previous")}
          icon={<ChevronLeft size={16} />}
          className={showLabels ? "px-3 py-1.5" : "w-8 h-8 p-0 shrink-0 flex items-center justify-center"}
        >
          {showLabels ? t("common.previous") : undefined}
        </SecondaryButton>

        <Flex align="center" gap={1}>
          {getVisiblePages().map((page, idx) =>
            typeof page === "number" ? (
              page === safePageNumber ? (
                <PrimaryButton
                  key={page}
                  type="button"
                  size="sm"
                  onClick={() => onPageChange(page)}
                  className="w-8 h-8 p-0 shrink-0 flex items-center justify-center text-xs font-semibold"
                >
                  {page}
                </PrimaryButton>
              ) : (
                <SecondaryButton
                  key={page}
                  type="button"
                  size="sm"
                  onClick={() => onPageChange(page)}
                  className="w-8 h-8 p-0 shrink-0 flex items-center justify-center text-xs font-semibold"
                >
                  {page}
                </SecondaryButton>
              )
            ) : (
              <Text key={`dots-${idx}`} size="xs" variant="muted" className="px-1 select-none">
                {page}
              </Text>
            )
          )}
        </Flex>

        <SecondaryButton
          type="button"
          size="sm"
          onClick={() => onPageChange(safePageNumber + 1)}
          disabled={safePageNumber >= safeTotalPages}
          aria-label={t("common.next")}
          title={t("common.next")}
          icon={showLabels ? undefined : <ChevronRight size={16} />}
          className={showLabels ? "px-3 py-1.5" : "w-8 h-8 p-0 shrink-0 flex items-center justify-center"}
        >
          {showLabels ? (
            <span className="flex items-center gap-1.5">
              {t("common.next")}
              <ChevronRight size={16} />
            </span>
          ) : undefined}
        </SecondaryButton>
      </Flex>
    </Flex>
  );
};

export default Pagination;
