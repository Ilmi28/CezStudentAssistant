import React from "react";
import { useTranslation } from "react-i18next";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Flex } from "./LayoutPrimitives";
import { Text } from "./Typography";
import { PrimaryButton, SecondaryButton } from "./Button";

export interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  onPageChange: (newPage: number) => void;
  showLabels?: boolean;
  className?: string;
}

export const Pagination: React.FC<PaginationProps> = ({
  pageNumber,
  totalPages,
  onPageChange,
  showLabels = false,
  className = "",
}) => {
  const { t } = useTranslation();

  if (totalPages <= 1) return null;

  const getVisiblePages = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (pageNumber > 3) {
        pages.push("...");
      }
      const start = Math.max(2, pageNumber - 1);
      const end = Math.min(totalPages - 1, pageNumber + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (pageNumber < totalPages - 2) {
        pages.push("...");
      }
      pages.push(totalPages);
    }
    return pages;
  };

  return (
    <Flex
      align="center"
      justify="between"
      className={`flex-col sm:flex-row gap-3 pt-5 border-t border-border ${className}`}
    >
      <Text size="xs" variant="muted" className="order-2 sm:order-1 font-medium">
        {t("common.paginationInfo", { current: pageNumber, total: totalPages })}
      </Text>

      <Flex align="center" gap={1.5} className="order-1 sm:order-2">
        <SecondaryButton
          type="button"
          size="sm"
          onClick={() => onPageChange(pageNumber - 1)}
          disabled={pageNumber <= 1}
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
              page === pageNumber ? (
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
          onClick={() => onPageChange(pageNumber + 1)}
          disabled={pageNumber >= totalPages}
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
