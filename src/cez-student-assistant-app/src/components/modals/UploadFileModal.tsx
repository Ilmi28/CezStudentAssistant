import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Upload, FileText, X } from "lucide-react";
import { Alert } from "../ui/Alert";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import Modal from "../ui/Modal";

interface UploadFileModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (file: File) => Promise<void>;
}

const MAX_FILE_SIZE_MB = 50;
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

const ALLOWED_EXTENSIONS = [
  ".pdf", ".docx", ".odt", ".pptx", ".odp", ".epub", ".rtf", ".html", ".htm",
  ".txt", ".md", ".csv", ".tsv", ".json", ".xml", ".yaml", ".yml",
  ".cs", ".js", ".ts", ".jsx", ".tsx", ".py", ".java", ".c", ".cpp", ".h", ".hpp", ".sql", ".sh", ".ps1", ".css",
  ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp",
  ".mp3", ".wav", ".ogg", ".m4a",
  ".mp4", ".webm", ".avi", ".mov"
];

const isFileSupported = (file: File) => {
  const ext = "." + file.name.split(".").pop()?.toLowerCase();
  return ALLOWED_EXTENSIONS.includes(ext) || file.type.startsWith("image/") || file.type.startsWith("text/") || file.type.startsWith("audio/") || file.type.startsWith("video/");
};

export default function UploadFileModal({
  isOpen,
  onClose,
  onSubmit,
}: UploadFileModalProps) {
  const { t } = useTranslation();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedFile) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    if (!isFileSupported(selectedFile)) {
      setModalError(t("courseDetails.unsupportedFormatError"));
      return;
    }
    if (selectedFile.size > MAX_FILE_SIZE_BYTES) {
      setModalError(t("courseDetails.fileTooLargeError", { maxSize: MAX_FILE_SIZE_MB }));
      return;
    }
    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(selectedFile);
      setSelectedFile(null);
      onClose();
    } catch (err: any) {
      if (err?.name === "PayloadTooLargeError" || err?.statusCode === 413 || err?.message?.includes("413")) {
        setModalError(t("courseDetails.fileTooLargeError", { maxSize: MAX_FILE_SIZE_MB }));
      } else {
        setModalError(err?.message || t("auth.genericError"));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    setSelectedFile(null);
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("courseDetails.uploadModalTitle")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        <div className="border-2 border-dashed border-border rounded-xl p-6 text-center hover:border-primary/45 transition-colors bg-muted/10">
          <Upload size={22} className="mx-auto text-muted-foreground/60 mb-2" />
          <label className="cursor-pointer block">
            <span className="text-xs text-foreground font-semibold block mb-1">
              {t("courseDetails.uploadPlaceholder")}
            </span>
            <span className="text-[10px] text-muted-foreground block">
              {t("courseDetails.uploadFormats", { maxSize: MAX_FILE_SIZE_MB })}
            </span>
            <input
              type="file"
              accept=".pdf,.docx,.odt,.pptx,.odp,.epub,.rtf,.html,.htm,.txt,.md,.csv,.tsv,.json,.xml,.yaml,.yml,.cs,.js,.ts,.jsx,.tsx,.py,.java,.c,.cpp,.h,.hpp,.sql,.sh,.ps1,.css,image/*,audio/*,video/*"
              onChange={(e) => {
                const file = e.target.files?.[0] || null;
                if (file) {
                  if (!isFileSupported(file)) {
                    setSelectedFile(null);
                    setModalError(t("courseDetails.unsupportedFormatError"));
                    return;
                  }
                  if (file.size > MAX_FILE_SIZE_BYTES) {
                    setSelectedFile(null);
                    setModalError(t("courseDetails.fileTooLargeError", { maxSize: MAX_FILE_SIZE_MB }));
                    return;
                  }
                }
                setSelectedFile(file);
                if (modalError) setModalError(null);
              }}
              className="hidden"
            />
          </label>
        </div>

        {selectedFile && (
          <div className="flex items-center justify-between p-3 rounded-xl bg-card border border-border">
            <div className="flex items-center gap-2.5 min-w-0">
              <div className="w-8 h-8 rounded-lg bg-muted/60 flex items-center justify-center text-muted-foreground shrink-0">
                <FileText size={16} />
              </div>
              <div className="min-w-0">
                <div className="text-xs font-semibold text-foreground truncate">{selectedFile.name}</div>
                <div className="text-[11px] text-muted-foreground">{(selectedFile.size / (1024 * 1024)).toFixed(2)} MB</div>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setSelectedFile(null)}
              className="p-1.5 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
            >
              <X size={16} />
            </button>
          </div>
        )}

        <div className="pt-2 flex gap-3">
          <SecondaryButton type="button" onClick={handleClose} className="flex-1">
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            disabled={!selectedFile}
            className="flex-1"
          >
            {t("courseDetails.uploadBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
