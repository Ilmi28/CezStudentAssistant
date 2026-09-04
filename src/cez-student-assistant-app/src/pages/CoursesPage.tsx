import { useState, useEffect } from "react";
import type { CourseDto } from "../services";
import { SyncBanner, CourseList, AddCourseModal } from "../components";
import { useCourse } from "../hooks";

interface CoursesPageProps {
  courses: CourseDto[];
  syncing: boolean;
  onSyncCourses: () => void;
  onCreateCourse: (name: string, description?: string) => Promise<void>;
  isCezConnected?: boolean;
  lastCezSync?: string | null;
}

export default function CoursesPage({
  courses,
  syncing,
  onSyncCourses,
  onCreateCourse,
  isCezConnected = false,
  lastCezSync,
}: CoursesPageProps) {
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const { refreshCourses } = useCourse();

  useEffect(() => {
    refreshCourses();
  }, []);

  return (
    <div className="space-y-8 animate-in fade-in duration-300">
      <SyncBanner
        syncing={syncing}
        onSyncCourses={onSyncCourses}
        isCezConnected={isCezConnected}
        lastCezSync={lastCezSync}
      />

      <CourseList
        courses={courses}
        onOpenAddModal={() => setIsAddModalOpen(true)}
      />

      <AddCourseModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onSubmit={onCreateCourse}
      />
    </div>
  );
}
