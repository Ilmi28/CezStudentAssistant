import React from "react";
import { useTranslation } from "react-i18next";
import { FileText, Check } from "lucide-react";
import Modal from "../ui/Modal";
import { PrimaryButton, SecondaryButton } from "../ui/Button";
import { Badge } from "../ui/Badge";
import type { CourseResourceDto } from "../../types";

interface AttachedResourcesModalProps {
  isOpen: boolean;
  onClose: () => void;
  resources: CourseResourceDto[];
  attachedResourceIds: string[];
  onToggleResource: (resourceId: string) => void;
  onSelectAll?: () => void;
  onDeselectAll?: () => void;
}

export const AttachedResourcesModal: React.FC<AttachedResourcesModalProps> = ({
  isOpen,
  onClose,
  resources,
  attachedResourceIds,
  onToggleResource,
  onSelectAll,
  onDeselectAll,
}) => {
  const { t } = useTranslation();

  const handleSelectAll = () => {
    if (onSelectAll) {
      onSelectAll();
    }
  };

  const handleDeselectAll = () => {
    if (onDeselectAll) {
      onDeselectAll();
    }
  };

  const attachedCount = resources.filter((r) => attachedResourceIds.includes(r.id)).length;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Dołączone materiały kursu"
      maxWidth="lg"
    >
      <div className="space-y-4">
        <p className="text-xs text-muted-foreground leading-relaxed">
          Wybierz materiały dydaktyczne, z których sztuczna inteligencja ma korzystać podczas odpowiadania w tym wątku czatu.
        </p>

        <div className="flex items-center justify-between gap-3 pt-1 border-t border-border">
          <span className="text-xs font-semibold text-foreground">
            Dołączono {attachedCount} z {resources.length} plików
          </span>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={handleSelectAll}
              className="text-xs text-primary hover:underline font-semibold cursor-pointer"
            >
              {t("common.selectAll", "Zaznacz wszystkie")}
            </button>
            <span className="text-muted-foreground text-xs">•</span>
            <button
              type="button"
              onClick={handleDeselectAll}
              className="text-xs text-muted-foreground hover:text-foreground font-medium cursor-pointer"
            >
              {t("common.deselectAll", "Odznacz wszystkie")}
            </button>
          </div>
        </div>

        <div className="max-h-72 overflow-y-auto space-y-2 pr-1">
          {resources.length === 0 ? (
            <div className="py-8 text-center text-xs text-muted-foreground">
              Brak plików w tym kursie.
            </div>
          ) : (
            resources.map((res) => {
              const isSelected = attachedResourceIds.includes(res.id);
              return (
                <div
                  key={res.id}
                  onClick={() => onToggleResource(res.id)}
                  className={`flex items-center justify-between p-3 rounded-xl border transition-colors cursor-pointer ${
                    isSelected
                      ? "bg-muted/40 border-primary/45 text-foreground"
                      : "bg-card border-border hover:border-border/80 text-muted-foreground"
                  }`}
                >
                  <div className="flex items-center gap-3 min-w-0 pr-2">
                    <div
                      className={`w-4 h-4 rounded flex items-center justify-center shrink-0 border transition-colors ${
                        isSelected
                          ? "bg-primary border-primary text-white"
                          : "border-border bg-background"
                      }`}
                    >
                      {isSelected && <Check size={11} strokeWidth={3} />}
                    </div>
                    <div className="flex items-center gap-2 min-w-0">
                      <FileText size={15} className="text-muted-foreground shrink-0" />
                      <span className="text-xs font-medium text-foreground truncate">
                        {res.displayName}
                      </span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    {res.isHidden && (
                      <Badge variant="secondary" className="text-[10px] px-2 py-0.5">
                        Ukryty w kursie
                      </Badge>
                    )}
                    <span
                      className={`text-[11px] font-semibold ${
                        isSelected ? "text-primary" : "text-muted-foreground"
                      }`}
                    >
                      {isSelected ? "Dołączony" : "Wyłączony"}
                    </span>
                  </div>
                </div>
              );
            })
          )}
        </div>

        <div className="pt-3 border-t border-border flex justify-end gap-3">
          <SecondaryButton type="button" onClick={onClose}>
            {t("common.cancel", "Anuluj")}
          </SecondaryButton>
          <PrimaryButton type="button" onClick={onClose}>
            {t("common.save", "Zapisz")}
          </PrimaryButton>
        </div>
      </div>
    </Modal>
  );
};

export default AttachedResourcesModal;
