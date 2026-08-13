import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Layers } from "lucide-react";
import type { CourseDto } from "../services";

interface CourseListProps {
  courses: CourseDto[];
}

export default function CourseList({ courses }: CourseListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div>
      <div className="mb-3 border-b border-border pb-2">
        <h3 className="text-sm font-bold uppercase tracking-wider text-foreground">
          {t("courses.title")}
        </h3>
      </div>

      {courses.length === 0 ? (
        <div className="bg-card rounded-xl border border-border p-8 text-center shadow-sm">
          <Layers size={32} className="mx-auto text-muted-foreground/30 mb-2" />
          <p className="text-sm text-muted-foreground">{t("courses.noCourses")}</p>
        </div>
      ) : (
        <div className="bg-card rounded-xl border border-border overflow-hidden shadow-sm">
          <div className="flex items-center justify-between px-5.5 py-3.5 bg-muted/60 border-b border-border text-xs uppercase tracking-wider text-muted-foreground font-bold font-sans">
            <span>{t("courses.thName")}</span>
          </div>
          {courses.map((c, i) => (
            <div
              key={c.id}
              onClick={() => navigate(`/course/${c.id}`)}
              className={`flex items-center justify-between px-5.5 py-4 hover:bg-muted/50 transition-colors cursor-pointer group ${
                i < courses.length - 1 ? "border-b border-border" : ""
              }`}
            >
              <div className="min-w-0 pr-4">
                <div className="text-sm font-medium text-foreground truncate group-hover:text-primary transition-colors">{c.name}</div>
              </div>
              {c.isCez !== false ? (
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20 shrink-0">
                  {t("courses.tagCez")}
                </span>
              ) : (
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-muted text-muted-foreground border border-border shrink-0">
                  {t("courses.tagCustom")}
                </span>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
