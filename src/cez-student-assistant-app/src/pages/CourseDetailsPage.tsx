import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ChevronLeft, FileText, Upload, X, RefreshCw, ChevronDown, Download } from "lucide-react";
import { courseService, type CourseDetailsDto } from "../services";
import { PrimaryButton } from "../components/Button";

interface CourseDetailsPageProps {
  setError: (msg: string) => void;
  setSuccess: (msg: string) => void;
}

export default function CourseDetailsPage({
  setError,
  setSuccess
}: CourseDetailsPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [selectedCourse, setSelectedCourse] = useState<CourseDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isFilesExpanded, setIsFilesExpanded] = useState(false);

  useEffect(() => {
    loadCourseDetails();
  }, [id]);

  const loadCourseDetails = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const details = await courseService.getCourseDetails(id);
      setSelectedCourse(details);
    } catch {
      setError(t("courseDetails.loadingDetails"));
      navigate("/courses");
    } finally {
      setLoading(false);
    }
  };

  const handleFileUploadSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile || !id) return;
    setUploading(true);
    try {
      await courseService.uploadCourseFile(id, selectedFile);
      setSuccess(t("courseDetails.uploadBtn") + " ✓");
      setSelectedFile(null);
      const details = await courseService.getCourseDetails(id);
      setSelectedCourse(details);
    } catch (err: any) {
      setError(err.message || t("courseDetails.uploadBtnLoading"));
    } finally {
      setUploading(false);
    }
  };

  const handleFileDownload = async (fileId: string, fileName: string) => {
    if (!id) return;
    try {
      await courseService.downloadCourseFile(id, fileId, fileName);
    } catch {
      setError(t("auth.genericError"));
    }
  };

  if (loading || !selectedCourse) {
    return (
      <div className="flex justify-center py-12">
        <div className="flex items-center gap-3 bg-card px-6 py-4 rounded-xl border border-border shadow-sm">
          <RefreshCw size={18} className="animate-spin text-primary" />
          <span className="text-xs font-medium text-muted-foreground">{t("courseDetails.loadingDetails")}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6 animate-in fade-in duration-300">
      {/* Header & Back Action */}
      <div className="flex items-center">
        <button
          onClick={() => navigate("/courses")}
          title={t("courseDetails.backBtn")}
          aria-label={t("courseDetails.backBtn")}
          className="w-10 h-10 rounded-xl bg-card border border-border flex items-center justify-center text-foreground hover:bg-muted hover:border-primary/40 hover:text-primary transition-colors shadow-xs cursor-pointer"
        >
          <ChevronLeft size={22} strokeWidth={2.25} className="shrink-0" />
        </button>
      </div>

      {/* Course Header Card */}
      <div className="bg-card rounded-xl border border-border p-6 shadow-sm">
        <div className="flex items-center justify-between">
          <div>
            {selectedCourse.isCez !== false ? (
              <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-primary/10 text-primary border border-primary/20">
                {t("courses.tagCez")}
              </span>
            ) : (
              <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-muted text-muted-foreground border border-border">
                {t("courses.tagCustom")}
              </span>
            )}
            <h1 className="text-xl font-bold text-foreground mt-2">
              {selectedCourse.name}
            </h1>
          </div>
        </div>
      </div>

      {/* Files List with Collapse/Expand */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <div
          onClick={() => setIsFilesExpanded(!isFilesExpanded)}
          className={`flex items-center justify-between cursor-pointer select-none group ${
            isFilesExpanded ? "pb-3.5 mb-2" : ""
          }`}
        >
          <h3 className="text-sm font-bold uppercase tracking-wider text-foreground group-hover:text-primary transition-colors">
            {t("courseDetails.filesTitle")} ({selectedCourse.files.length})
          </h3>
          <button className="text-muted-foreground group-hover:text-primary transition-colors p-1 rounded-lg hover:bg-muted cursor-pointer">
            <ChevronDown
              size={18}
              className={`transition-transform duration-300 ${isFilesExpanded ? "rotate-180" : ""}`}
            />
          </button>
        </div>

        <div
          className={`grid transition-all duration-300 ease-in-out ${
            isFilesExpanded ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0 pointer-events-none"
          }`}
        >
          <div className="overflow-hidden">
            {selectedCourse.files.length === 0 ? (
              <div className="py-8 text-center">
                <FileText size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                <p className="text-xs text-muted-foreground">{t("courseDetails.noFiles")}</p>
              </div>
            ) : (
              <div className="space-y-2">
                {selectedCourse.files.map((file) => (
                  <div
                    key={file.id}
                    className="flex items-center justify-between p-3.5 rounded-xl bg-muted/40 border border-border hover:border-primary/30 transition-all"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="w-8 h-8 rounded-lg bg-card border border-border flex items-center justify-center text-primary flex-shrink-0">
                        <FileText size={15} />
                      </div>
                      <div className="min-w-0">
                        <div className="text-xs font-medium text-foreground truncate">{file.displayName}</div>
                        <div className="text-[11px] text-muted-foreground/50">{file.mimeType}</div>
                      </div>
                    </div>
                    <button
                      onClick={() => handleFileDownload(file.id, file.displayName)}
                      title={t("courseDetails.downloadFile")}
                      aria-label={t("courseDetails.downloadFile")}
                      className="p-2 rounded-lg text-muted-foreground hover:text-primary hover:bg-muted transition-colors shrink-0 ml-2 cursor-pointer"
                    >
                      <Download size={18} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* File Upload Section */}
      <div className="bg-card rounded-xl border border-border p-5 shadow-sm">
        <h3 className="text-sm font-bold uppercase tracking-wider text-foreground border-b border-border pb-2.5 mb-4">
          {t("courseDetails.uploadTitle")}
        </h3>
        <form onSubmit={handleFileUploadSubmit} className="space-y-4">
          <div className="border-2 border-dashed border-border rounded-xl p-6 text-center hover:border-primary/40 transition-colors bg-muted/10">
            <Upload size={24} className="mx-auto text-muted-foreground/50 mb-2" />
            <label className="cursor-pointer block">
              <span className="text-xs text-primary font-semibold hover:underline block mb-1">
                {t("courseDetails.uploadPlaceholder")}
              </span>
              <span className="text-[10px] text-muted-foreground block">{t("courseDetails.uploadFormats")}</span>
              <input
                type="file"
                onChange={(e) => setSelectedFile(e.target.files?.[0] || null)}
                className="hidden"
              />
            </label>
            {selectedFile && (
              <div className="mt-3 bg-muted px-3 py-1.5 rounded-lg inline-flex items-center gap-2 text-xs text-foreground border border-border">
                <FileText size={12} />
                <span className="font-medium truncate max-w-[180px]">{selectedFile.name}</span>
                <button type="button" onClick={() => setSelectedFile(null)} className="hover:opacity-80 cursor-pointer">
                  <X size={12} />
                </button>
              </div>
            )}
          </div>
          <PrimaryButton
            type="submit"
            fullWidth
            loading={uploading}
            disabled={!selectedFile}
            icon={!uploading ? <Upload size={16} /> : undefined}
          >
            {t("courseDetails.uploadBtn")}
          </PrimaryButton>
        </form>
      </div>
    </div>
  );
}
