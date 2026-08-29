import { useState, useEffect } from "react";
import type { CourseDto } from "../services";
import SyncBanner from "../components/SyncBanner";
import CourseList from "../components/CourseList";
import AddCourseModal from "../components/AddCourseModal";
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
      {/* Sync Banner component */}
      <SyncBanner
        syncing={syncing}
        onSyncCourses={onSyncCourses}
        isCezConnected={isCezConnected}
        lastCezSync={lastCezSync}
      />

      {/* Course List component */}
      <CourseList
        courses={courses}
        onOpenAddModal={() => setIsAddModalOpen(true)}
      />

      {/* Add Course Modal */}
      <AddCourseModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onSubmit={onCreateCourse}
      />
    </div>
  );
}
