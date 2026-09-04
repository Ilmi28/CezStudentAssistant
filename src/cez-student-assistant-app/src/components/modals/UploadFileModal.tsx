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
    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(selectedFile);
      setSelectedFile(null);
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
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
      title={t("courseDetails.uploadTitle")}
      icon={<Upload size={20} className="text-primary" />}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        <div className="border-2 border-dashed border-border rounded-xl p-6 text-center hover:border-border/80 transition-colors bg-muted/10">
          <Upload size={24} className="mx-auto text-muted-foreground/50 mb-2" />
          <label className="cursor-pointer block">
            <span className="text-xs text-primary font-semibold hover:underline block mb-1">
              {t("courseDetails.uploadPlaceholder")}
            </span>
            <span className="text-[10px] text-muted-foreground block">
              {t("courseDetails.uploadFormats")}
            </span>
            <input
              type="file"
              accept=".pdf,.docx,.odt,.pptx,.odp,.epub,.rtf,.html,.htm,.txt,.md,.csv,.tsv,.json,.xml,.yaml,.yml,.cs,.js,.ts,.jsx,.tsx,.py,.java,.c,.cpp,.h,.hpp,.sql,.sh,.ps1,.css,image/*,audio/*,video/*"
              onChange={(e) => {
                const file = e.target.files?.[0] || null;
                if (file && !isFileSupported(file)) {
                  setSelectedFile(null);
                  setModalError(t("courseDetails.unsupportedFormatError"));
                  return;
                }
                setSelectedFile(file);
                if (modalError) setModalError(null);
              }}
              className="hidden"
            />
          </label>

          {selectedFile && (
            <div className="mt-3 bg-muted px-3 py-1.5 rounded-lg inline-flex items-center gap-2 text-xs text-foreground border border-border">
              <FileText size={14} className="text-primary" />
              <span className="font-medium truncate max-w-[200px]">{selectedFile.name}</span>
              <button
                type="button"
                onClick={() => setSelectedFile(null)}
                className="hover:opacity-80 cursor-pointer ml-1 text-muted-foreground hover:text-foreground"
              >
                <X size={14} />
              </button>
            </div>
          )}
        </div>

        <div className="pt-2 flex gap-3">
          <SecondaryButton type="button" onClick={handleClose} className="flex-1">
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            disabled={!selectedFile}
            icon={!loading ? <Upload size={16} /> : undefined}
            className="flex-1"
          >
            {t("courseDetails.uploadBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
