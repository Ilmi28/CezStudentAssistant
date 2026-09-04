import React from "react";

export interface FlexProps extends React.HTMLAttributes<HTMLDivElement> {
  direction?: "row" | "col" | "row-reverse" | "col-reverse";
  align?: "start" | "center" | "end" | "stretch" | "baseline";
  justify?: "start" | "center" | "end" | "between" | "around" | "evenly";
  gap?: number | "none" | "xs" | "sm" | "md" | "lg" | "xl";
  wrap?: boolean;
  children: React.ReactNode;
}

const directionStyles = {
  row: "flex-row",
  col: "flex-col",
  "row-reverse": "flex-row-reverse",
  "col-reverse": "flex-col-reverse",
};

const alignStyles = {
  start: "items-start",
  center: "items-center",
  end: "items-end",
  stretch: "items-stretch",
  baseline: "items-baseline",
};

const justifyStyles = {
  start: "justify-start",
  center: "justify-center",
  end: "justify-end",
  between: "justify-between",
  around: "justify-around",
  evenly: "justify-evenly",
};

const gapStyles: Record<string, string> = {
  none: "gap-0",
  xs: "gap-1",
  sm: "gap-2",
  md: "gap-4",
  lg: "gap-6",
  xl: "gap-8",
};

const numGapStyles: Record<number, string> = {
  0: "gap-0",
  0.5: "gap-0.5",
  1: "gap-1",
  1.5: "gap-1.5",
  2: "gap-2",
  2.5: "gap-2.5",
  3: "gap-3",
  3.5: "gap-3.5",
  4: "gap-4",
  5: "gap-5",
  6: "gap-6",
  8: "gap-8",
};

function getGapClass(gap: FlexProps["gap"]): string {
  if (typeof gap === "number") {
    return numGapStyles[gap] || `gap-${gap}`;
  }
  return gapStyles[gap || "none"] || "";
}

export const Flex: React.FC<FlexProps> = ({
  direction = "row",
  align = "stretch",
  justify = "start",
  gap = "none",
  wrap = false,
  children,
  className = "",
  ...props
}) => {
  return (
    <div
      className={`flex ${directionStyles[direction]} ${alignStyles[align]} ${justifyStyles[justify]} ${getGapClass(
        gap
      )} ${wrap ? "flex-wrap" : ""} ${className}`.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {children}
    </div>
  );
};

export interface GridProps extends React.HTMLAttributes<HTMLDivElement> {
  cols?: number;
  smCols?: number;
  mdCols?: number;
  lgCols?: number;
  gap?: number | "none" | "xs" | "sm" | "md" | "lg" | "xl";
  children: React.ReactNode;
}

const colsMap: Record<number, string> = {
  1: "grid-cols-1",
  2: "grid-cols-2",
  3: "grid-cols-3",
  4: "grid-cols-4",
  6: "grid-cols-6",
  12: "grid-cols-12",
};

const smColsMap: Record<number, string> = {
  1: "sm:grid-cols-1",
  2: "sm:grid-cols-2",
  3: "sm:grid-cols-3",
  4: "sm:grid-cols-4",
  6: "sm:grid-cols-6",
  12: "sm:grid-cols-12",
};

const mdColsMap: Record<number, string> = {
  1: "md:grid-cols-1",
  2: "md:grid-cols-2",
  3: "md:grid-cols-3",
  4: "md:grid-cols-4",
  6: "md:grid-cols-6",
  12: "md:grid-cols-12",
};

const lgColsMap: Record<number, string> = {
  1: "lg:grid-cols-1",
  2: "lg:grid-cols-2",
  3: "lg:grid-cols-3",
  4: "lg:grid-cols-4",
  6: "lg:grid-cols-6",
  12: "lg:grid-cols-12",
};

export const Grid: React.FC<GridProps> = ({
  cols = 1,
  smCols,
  mdCols,
  lgCols,
  gap = "md",
  children,
  className = "",
  ...props
}) => {
  const baseCol = colsMap[cols] || `grid-cols-${cols}`;
  const smCol = smCols ? smColsMap[smCols] || `sm:grid-cols-${smCols}` : "";
  const mdCol = mdCols ? mdColsMap[mdCols] || `md:grid-cols-${mdCols}` : "";
  const lgCol = lgCols ? lgColsMap[lgCols] || `lg:grid-cols-${lgCols}` : "";
  const gapClass = getGapClass(gap);

  return (
    <div
      className={`grid ${baseCol} ${smCol} ${mdCol} ${lgCol} ${gapClass} ${className}`
        .replace(/\s+/g, ' ')
        .trim()}
      {...props}
    >
      {children}
    </div>
  );
};
