import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Input } from "./Input";
import { Alert } from "./Alert";
import { PrimaryButton, SecondaryButton } from "./Button";
import Modal from "./Modal";

interface AddCourseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (name: string, description?: string) => Promise<void>;
}

export default function AddCourseModal({
  isOpen,
  onClose,
  onSubmit,
}: AddCourseModalProps) {
  const { t } = useTranslation();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [modalError, setModalError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setModalError(t("auth.emptyFields"));
      return;
    }
    setModalError(null);
    setLoading(true);
    try {
      await onSubmit(name.trim(), description.trim() || undefined);
      setName("");
      setDescription("");
      onClose();
    } catch (err: any) {
      setModalError(err.message || t("auth.genericError"));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setModalError(null);
    setName("");
    setDescription("");
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={t("courses.addCourseModalTitle")}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Alert message={modalError} />

        <Input
          label={t("courses.courseNameLabel")}
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            if (modalError) setModalError(null);
          }}
        />

        <Input
          label={t("courses.courseDescLabel")}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />

        <div className="pt-2 flex gap-3">
          <SecondaryButton
            type="button"
            onClick={handleClose}
            className="flex-1"
          >
            {t("common.cancel")}
          </SecondaryButton>
          <PrimaryButton
            type="submit"
            loading={loading}
            className="flex-1"
          >
            {t("courses.addCourseBtn")}
          </PrimaryButton>
        </div>
      </form>
    </Modal>
  );
}
