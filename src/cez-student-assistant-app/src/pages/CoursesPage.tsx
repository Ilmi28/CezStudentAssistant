import type { CourseDto } from "../services/api";
import SyncBanner from "../components/SyncBanner";
import CourseList from "../components/CourseList";

interface CoursesPageProps {
  courses: CourseDto[];
  syncing: boolean;
  onSyncCourses: () => void;
  isCezConnected?: boolean;
  lastCezSync?: string | null;
}

export default function CoursesPage({
  courses,
  syncing,
  onSyncCourses,
  isCezConnected = false,
  lastCezSync,
}: CoursesPageProps) {
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
      <CourseList courses={courses} />
    </div>
  );
}
