import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronRight, Layers } from "lucide-react";
import type { CourseDto } from "../services";

interface CourseListProps {
  courses: CourseDto[];
}

export default function CourseList({ courses }: CourseListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div>
      <div className="flex items-center justify-between mb-3 border-b border-border pb-2">
        <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground">
          {t("courses.title")}
        </h3>
        <span className="text-[10px] text-muted-foreground font-mono">{t("courses.subtitle")}</span>
      </div>

      {courses.length === 0 ? (
        <div className="bg-card rounded-lg border border-border p-8 text-center shadow-sm">
          <Layers size={32} className="mx-auto text-muted-foreground/30 mb-2" />
          <p className="text-[13px] text-muted-foreground">{t("courses.noCourses")}</p>
        </div>
      ) : (
        <div className="bg-card rounded-lg border border-border overflow-hidden shadow-sm">
          <div className="grid grid-cols-[1fr_10rem_3rem] gap-4 px-5 py-3 bg-muted border-b border-border text-[10px] uppercase tracking-wider text-muted-foreground font-bold font-sans">
            <span>{t("courses.thName")}</span>
            <span className="text-right">{t("courses.thSyncDate")}</span>
            <span />
          </div>
          {courses.map((c, i) => (
            <div
              key={c.id}
              onClick={() => navigate(`/course/${c.id}`)}
              className={`grid grid-cols-[1fr_10rem_3rem] gap-4 px-5 py-4 items-center hover:bg-muted/50 transition-colors cursor-pointer group ${
                i < courses.length - 1 ? "border-b border-border" : ""
              }`}
            >
              <div className="min-w-0">
                <div className="text-[13px] font-medium text-foreground truncate group-hover:text-primary transition-colors">{c.name}</div>
                <div className="text-[10px] text-muted-foreground/50 font-mono mt-0.5">ID: {c.id}</div>
              </div>
              <div className="text-right text-[12px] text-muted-foreground font-mono">
                {c.lastSynched ? new Date(c.lastSynched).toLocaleString("pl-PL", { dateStyle: "short", timeStyle: "short" }) : t("courses.noSyncDate")}
              </div>
              <ChevronRight size={15} className="text-muted-foreground/30 group-hover:text-primary transition-colors justify-self-end" />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
