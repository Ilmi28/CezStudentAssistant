import { memo } from "react";
import ReactMarkdown from "react-markdown";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { vscDarkPlus, vs } from "react-syntax-highlighter/dist/esm/styles/prism";
import { useUI } from "../../hooks";

interface MarkdownRendererProps {
  content: string;
  className?: string;
  isUser?: boolean;
}

export const MarkdownRenderer = memo(function MarkdownRenderer({
  content,
  className = "",
  isUser = false,
}: MarkdownRendererProps) {
  const { darkMode } = useUI();

  return (
    <div className={`markdown-content text-sm leading-relaxed ${className}`}>
      <ReactMarkdown
        components={{
          h1: ({ children }) => (
            <h1 className="text-base font-bold my-2.5 text-foreground">{children}</h1>
          ),
          h2: ({ children }) => (
            <h2 className="text-sm font-bold my-2 text-foreground">{children}</h2>
          ),
          h3: ({ children }) => (
            <h3 className="text-xs font-bold my-1.5 text-foreground">{children}</h3>
          ),
          p: ({ children }) => <p className="mb-1.5 last:mb-0">{children}</p>,
          strong: ({ children }) => (
            <strong
              className={
                isUser ? "font-bold text-white" : "font-semibold text-foreground dark:text-white"
              }
            >
              {children}
            </strong>
          ),
          em: ({ children }) => <em className="italic">{children}</em>,
          ul: ({ children }) => (
            <ul className="list-disc pl-5 my-2 space-y-1">{children}</ul>
          ),
          ol: ({ children }) => (
            <ol className="list-decimal pl-5 my-2 space-y-1">{children}</ol>
          ),
          li: ({ children }) => <li className="leading-snug">{children}</li>,
          code: ({ className: codeClassName, children, ...props }) => {
            const match = /language-(\w+)/.exec(codeClassName || "");
            const isInline = !codeClassName && !String(children).includes("\n");

            if (isInline) {
              return (
                <code
                  className={
                    isUser
                      ? "px-1.5 py-0.5 rounded bg-white/20 text-white font-mono text-[0.85em]"
                      : "px-1.5 py-0.5 rounded bg-secondary text-secondary-foreground dark:bg-sidebar dark:text-sky-300 border border-border/60 font-mono text-[0.85em]"
                  }
                  {...props}
                >
                  {children}
                </code>
              );
            }

            const language = match ? match[1] : "text";
            const codeString = String(children).replace(/\n$/, "");

            return (
              <div className="my-3 rounded-xl overflow-hidden border border-border/80 shadow-2xs">
                <SyntaxHighlighter
                  language={language}
                  style={darkMode ? vscDarkPlus : vs}
                  customStyle={{
                    margin: 0,
                    padding: "0.875rem 1rem",
                    fontSize: "0.775rem",
                    lineHeight: "1.5",
                    background: darkMode ? "#182238" : "#f8fafc",
                  }}
                  codeTagProps={{
                    style: {
                      fontFamily: "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
                    },
                  }}
                >
                  {codeString}
                </SyntaxHighlighter>
              </div>
            );
          },
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
});

export default MarkdownRenderer;
