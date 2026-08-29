import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers, Plus } from "lucide-react";
import { SecondaryButton } from "./Button";
import type { CourseDto } from "../services";

interface CourseListProps {
  courses: CourseDto[];
  onOpenAddModal?: () => void;
}

export default function CourseList({ courses, onOpenAddModal }: CourseListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const sortedCourses = [...courses].sort((a, b) => a.name.localeCompare(b.name));

  return (
    <div>
      <div className="mb-3 border-b border-border pb-2.5 flex items-center justify-between">
        <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
          {t("courses.title")}
        </h3>
        {onOpenAddModal && (
          <SecondaryButton
            type="button"
            onClick={onOpenAddModal}
            title={t("courses.addCourseBtn")}
            aria-label={t("courses.addCourseBtn")}
            className="w-10 h-10 p-0 flex items-center justify-center shrink-0"
          >
            <Plus size={22} strokeWidth={2.25} className="shrink-0" />
          </SecondaryButton>
        )}
      </div>


      {sortedCourses.length === 0 ? (
        <div className="bg-card rounded-xl border border-border p-8 text-center shadow-sm">
          <Layers size={32} className="mx-auto text-muted-foreground/30 mb-2" />
          <p className="text-sm text-muted-foreground">{t("courses.noCourses")}</p>
        </div>
      ) : (
        <div className="space-y-3">
          {sortedCourses.map((c) => (
            <div
              key={c.id}
              onClick={() => navigate(`/course/${c.id}`)}
              className="bg-card rounded-xl border border-border px-5.5 py-4 hover:border-primary/40 hover:shadow-md transition-all cursor-pointer group flex items-center justify-between shadow-xs"
            >
              <div className="min-w-0 pr-4">
                <div className="text-sm font-medium text-foreground truncate group-hover:text-primary transition-colors">
                  {c.name}
                </div>
              </div>
              {c.isCez && (
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-secondary border border-border text-foreground/90 shrink-0 shadow-2xs">
                  {t("courses.tagCez")}
                </span>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
