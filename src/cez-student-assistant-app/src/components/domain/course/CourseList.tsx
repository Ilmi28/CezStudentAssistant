import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Plus } from "lucide-react";
import { Card, Badge, Heading, Text, Flex, SecondaryButton, Tooltip } from "../../index";
import type { CourseDto } from "../../../services";

interface CourseListProps {
  courses: CourseDto[];
  onOpenAddModal?: () => void;
}

export default function CourseList({ courses, onOpenAddModal }: CourseListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const sortedCourses = [...courses].sort((a, b) => a.name.localeCompare(b.name));

  return (
    <div className="space-y-3">
      <Flex align="center" justify="between" className="border-b border-border pb-2.5 mb-3">
        <Heading level={3} size="sm" uppercase className="tracking-wider">
          {t("courses.title")}
        </Heading>
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

      {sortedCourses.length === 0 ? (
        <Card className="p-8 text-center shadow-sm">
          <Layers size={32} className="mx-auto text-muted-foreground/30 mb-2" />
          <Text size="sm" variant="muted">{t("courses.noCourses")}</Text>
        </Card>
      ) : (
        <div className="space-y-3">
          {sortedCourses.map((c) => (
            <Card
              key={c.id}
              hoverEffect
              onClick={() => navigate(`/course/${c.id}`)}
              className="px-5.5 py-4 cursor-pointer flex-row items-center justify-between shadow-xs"
            >
              <div className="min-w-0 pr-4">
                <Text size="sm" className="font-medium text-foreground truncate">
                  {c.name}
                </Text>
              </div>
              {c.isCez && (
                <Badge variant="secondary" className="shrink-0">
                  {t("courses.tagCez")}
                </Badge>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
