import { useState, useRef, useEffect, type ReactNode } from "react";
import { ChevronDown, Check } from "lucide-react";

export interface SelectOption<T extends string | number> {
  value: T;
  label: string | ReactNode;
}

export interface SelectProps<T extends string | number> {
  id?: string;
  value: T;
  options: SelectOption<T>[];
  onChange: (value: T) => void;
  disabled?: boolean;
  className?: string;
  placeholder?: string;
}

export function Select<T extends string | number>({
  id,
  value,
  options,
  onChange,
  disabled = false,
  className = "",
  placeholder,
}: SelectProps<T>) {
  const [isOpen, setIsOpen] = useState(false);
  const [shouldRender, setShouldRender] = useState(false);
  const [isClosing, setIsClosing] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find((opt) => opt.value === value);
  const hasScroll = options.length > 5;

  useEffect(() => {
    if (isOpen) {
      setShouldRender(true);
      setIsClosing(false);
    } else if (shouldRender && !isClosing) {
      setIsClosing(true);
      const timer = setTimeout(() => {
        setShouldRender(false);
        setIsClosing(false);
      }, 140);
      return () => clearTimeout(timer);
    }
  }, [isOpen]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    }

    if (shouldRender) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [shouldRender]);

  const handleSelect = (optionValue: T) => {
    onChange(optionValue);
    setIsOpen(false);
  };

  return (
    <div className={`relative w-full ${className}`} ref={containerRef}>
      <button
        id={id}
        type="button"
        disabled={disabled}
        onClick={() => setIsOpen((prev) => !prev)}
        className={`w-full flex items-center justify-between px-4 py-2.5 bg-card border border-border rounded-xl text-foreground text-sm font-medium cursor-pointer transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-primary/40 focus:border-primary ${
          disabled ? "opacity-50 cursor-not-allowed" : "hover:border-primary/50 hover:bg-muted/30"
        } ${isOpen ? "ring-2 ring-primary/40 border-primary bg-card" : ""}`}
        aria-expanded={isOpen}
        aria-haspopup="listbox"
      >
        <span className="truncate">
          {selectedOption ? selectedOption.label : placeholder || String(value)}
        </span>
        <ChevronDown
          size={16}
          className={`text-muted-foreground transition-transform duration-200 shrink-0 ml-2 ${
            isOpen ? "rotate-180 text-primary" : ""
          }`}
        />
      </button>

      {shouldRender && (
        <div
          role="listbox"
          className={`absolute left-0 right-0 w-full top-full mt-1.5 rounded-xl bg-card border border-border shadow-xl p-1 z-50 ${
            hasScroll ? "max-h-60 overflow-y-auto" : "overflow-hidden"
          } ${isClosing ? "animate-dropdown-exit" : "animate-dropdown-enter"}`}
        >
          {options.map((option) => {
            const isSelected = option.value === value;
            return (
              <div
                key={String(option.value)}
                role="option"
                aria-selected={isSelected}
                onClick={() => handleSelect(option.value)}
                className={`w-full px-3 py-2 text-sm font-medium rounded-lg cursor-pointer transition-colors flex items-center justify-between ${
                  isSelected
                    ? "bg-primary/10 text-primary font-semibold"
                    : "text-foreground hover:bg-muted"
                }`}
              >
                <span className="truncate">{option.label}</span>
                {isSelected && <Check size={16} className="text-primary shrink-0 ml-2" />}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default Select;
