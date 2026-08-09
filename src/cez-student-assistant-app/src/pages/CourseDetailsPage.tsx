import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft, FileText, Upload, Brain, X, RefreshCw } from "lucide-react";
import { api, type CourseDetailsDto } from "../services/api";
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
  const [generating, setGenerating] = useState(false);

  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [questionCount, setQuestionCount] = useState<number>(5);
  const [additionalInstructions, setAdditionalInstructions] = useState("");

  useEffect(() => {
    loadCourseDetails();
  }, [id]);

  const loadCourseDetails = async () => {
    if (!id) return;
    setLoading(true);
    try {
      const details = await api.getCourseDetails(id);
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
      await api.uploadCourseFile(id, selectedFile);
      setSuccess(t("courseDetails.uploadBtn") + " ✓");
      setSelectedFile(null);
      const details = await api.getCourseDetails(id);
      setSelectedCourse(details);
    } catch (err: any) {
      setError(err.message || t("courseDetails.uploadBtnLoading"));
    } finally {
      setUploading(false);
    }
  };

  const handleGenerateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    setGenerating(true);
    try {
      await api.generateQuiz(id, questionCount, additionalInstructions);
      setSuccess(t("courseDetails.generateBtn") + " ✓");
      setAdditionalInstructions("");
    } catch (err: any) {
      setError(err.message || t("courseDetails.generateBtnLoading"));
    } finally {
      setGenerating(false);
    }
  };

  if (loading || !selectedCourse) {
    return (
      <div className="flex justify-center py-12">
        <div className="flex items-center gap-3 bg-card px-6 py-4 rounded-lg border border-border shadow-sm">
          <RefreshCw size={18} className="animate-spin text-primary" />
          <span className="text-[13px] font-medium text-muted-foreground">{t("courseDetails.loadingDetails")}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      {/* Header & Back Action */}
      <div className="flex items-center gap-2">
        <button
          onClick={() => navigate("/courses")}
          className="inline-flex items-center gap-1.5 text-[12px] text-primary hover:underline font-semibold cursor-pointer"
        >
          <ArrowLeft size={14} /> {t("courseDetails.backBtn")}
        </button>
      </div>

      {/* Course Card */}
      <div className="bg-card rounded-lg border border-border p-6 shadow-sm">
        <div className="flex items-center justify-between border-b border-border pb-3 mb-4">
          <div>
            <span className="text-[9px] bg-primary/10 text-primary font-bold px-2 py-0.5 rounded uppercase">
              {t("courseDetails.tag")}
            </span>
            <h1 style={{ fontFamily: "Roboto Slab, serif" }} className="text-xl font-bold text-foreground mt-1.5">
              {selectedCourse.name}
            </h1>
          </div>
          <div className="text-right text-[11px] text-muted-foreground font-mono">
            {t("courseDetails.thType")}: {selectedCourse.type === 0 ? t("courseDetails.lecture") : t("courseDetails.laboratory")}
          </div>
        </div>
        <p className="text-[13px] text-muted-foreground leading-relaxed">
          {selectedCourse.description || t("courseDetails.noDesc")}
        </p>
        <div className="mt-4 pt-3 border-t border-border flex justify-between items-center text-[11px] text-muted-foreground/60">
          <span>{t("courseDetails.lastSynched", { date: new Date(selectedCourse.lastSynched).toLocaleString() })}</span>
          <span className="font-mono">ID: {selectedCourse.id}</span>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left: Files and Upload */}
        <div className="lg:col-span-2 space-y-6">
          {/* Files list */}
          <div className="bg-card rounded-lg border border-border p-5 shadow-sm">
            <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground border-b border-border pb-2.5 mb-4">
              {t("courseDetails.filesTitle")}
            </h3>

            {selectedCourse.files.length === 0 ? (
              <div className="py-8 text-center">
                <FileText size={28} className="mx-auto text-muted-foreground/35 mb-2" />
                <p className="text-[12px] text-muted-foreground">{t("courseDetails.noFiles")}</p>
              </div>
            ) : (
              <div className="space-y-2">
                {selectedCourse.files.map((file) => (
                  <div
                    key={file.id}
                    className="flex items-center justify-between p-3 rounded bg-muted/40 border border-border hover:border-primary/30 transition-all"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="w-8 h-8 rounded bg-card border border-border flex items-center justify-center text-primary flex-shrink-0">
                        <FileText size={15} />
                      </div>
                      <div className="min-w-0">
                        <div className="text-[12px] font-medium text-foreground truncate">{file.name}</div>
                        <div className="text-[9px] text-muted-foreground/50 font-mono">{file.contentType} · ID: {file.id.substring(0, 8)}</div>
                      </div>
                    </div>
                    <a
                      href={`/course/${selectedCourse.id}/file/${file.id}/download`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-[12px] text-primary hover:underline font-bold flex-shrink-0 ml-4"
                    >
                      {t("courseDetails.download")}
                    </a>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* File upload */}
          <div className="bg-card rounded-lg border border-border p-5 shadow-sm">
            <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground border-b border-border pb-2.5 mb-4">
              {t("courseDetails.uploadTitle")}
            </h3>
            <form onSubmit={handleFileUploadSubmit} className="space-y-4">
              <div className="border-2 border-dashed border-border rounded-lg p-6 text-center hover:border-primary/40 transition-colors bg-muted/10">
                <Upload size={24} className="mx-auto text-muted-foreground/50 mb-2" />
                <label className="cursor-pointer block">
                  <span className="text-[12px] text-primary font-semibold hover:underline block mb-1">
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
                  <div className="mt-3 bg-muted px-3 py-1.5 rounded inline-flex items-center gap-2 text-[11px] text-foreground border border-border">
                    <FileText size={12} />
                    <span className="font-medium truncate max-w-[180px]">{selectedFile.name}</span>
                    <button type="button" onClick={() => setSelectedFile(null)} className="hover:opacity-80">
                      <X size={12} />
                    </button>
                  </div>
                )}
              </div>
              <PrimaryButton
                type="submit"
                size="sm"
                fullWidth
                loading={uploading}
                disabled={!selectedFile}
                icon={!uploading ? <Upload size={13} /> : undefined}
              >
                {t("courseDetails.uploadBtn")}
              </PrimaryButton>
            </form>
          </div>
        </div>

        {/* Right: AI Quiz generator */}
        <div className="space-y-6">
          <div className="bg-card rounded-lg border border-border p-5 shadow-sm">
            <div className="inline-flex w-7 h-7 rounded bg-primary items-center justify-center mb-3">
              <Brain size={14} className="text-white" />
            </div>
            <h3 style={{ fontFamily: "Roboto Slab, serif" }} className="text-[14px] font-bold uppercase tracking-wider text-foreground border-b border-border pb-2.5 mb-3">
              {t("courseDetails.generateTitle")}
            </h3>
            <p className="text-[12px] text-muted-foreground mb-4">
              {t("courseDetails.generateDesc")}
            </p>
            
            <form onSubmit={handleGenerateSubmit} className="space-y-4">
              <div>
                <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-medium mb-1">
                  {t("courseDetails.generateQuestionsCount")}
                </label>
                <select
                  value={questionCount}
                  onChange={(e) => setQuestionCount(Number(e.target.value))}
                  className="w-full px-3 py-2 text-[12px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary"
                >
                  <option value={3}>{t("courseDetails.generateQuestionsCountOptions.3")}</option>
                  <option value={5}>{t("courseDetails.generateQuestionsCountOptions.5")}</option>
                  <option value={10}>{t("courseDetails.generateQuestionsCountOptions.10")}</option>
                  <option value={15}>{t("courseDetails.generateQuestionsCountOptions.15")}</option>
                </select>
              </div>
              <div>
                <label className="block text-[11px] uppercase tracking-wider text-muted-foreground font-medium mb-1">
                  {t("courseDetails.generateInstructions")}
                </label>
                <textarea
                  value={additionalInstructions}
                  onChange={(e) => setAdditionalInstructions(e.target.value)}
                  rows={3}
                  className="w-full px-3 py-2 text-[12px] border border-border rounded bg-card text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary"
                  placeholder={t("courseDetails.generateInstructionsPlaceholder")}
                />
              </div>

              <PrimaryButton
                type="submit"
                size="sm"
                fullWidth
                loading={generating}
                disabled={selectedCourse.files.length === 0}
                icon={!generating ? <Brain size={14} /> : undefined}
              >
                {t("courseDetails.generateBtn")}
              </PrimaryButton>
              {selectedCourse.files.length === 0 && (
                <p className="text-[10px] text-[#c44444] text-center font-medium mt-1">
                  {t("courseDetails.generateNoFilesError")}
                </p>
              )}
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
