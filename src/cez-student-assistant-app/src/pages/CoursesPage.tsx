import React, { useState, useEffect } from "react";
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
  const [searchTerm, setSearchTerm] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const fetchCourses = async (page: number, search: string) => {
    const res = await refreshCourses(page, 10, search);
    if (res) {
      setTotalPages(res.totalPages);
      setTotalCount(res.totalCount);
    }
  };

  useEffect(() => {
    fetchCourses(pageNumber, searchTerm);
  }, [pageNumber, searchTerm]);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setPageNumber(1);
  };

  const handleClearSearch = () => {
    setSearchTerm("");
    setPageNumber(1);
  };

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
        totalCount={totalCount}
        pageNumber={pageNumber}
        totalPages={totalPages}
        searchTerm={searchTerm}
        onSearchChange={handleSearchChange}
        onClearSearch={handleClearSearch}
        onPageChange={setPageNumber}
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
