import React from "react";

export interface HeadingProps extends React.HTMLAttributes<HTMLHeadingElement> {
  level?: 1 | 2 | 3 | 4 | 5 | 6;
  size?: "xs" | "sm" | "base" | "lg" | "xl" | "2xl" | "3xl";
  uppercase?: boolean;
  children: React.ReactNode;
}

const headingStyles = {
  1: "text-2xl sm:text-3xl font-bold text-foreground tracking-tight",
  2: "text-xl sm:text-2xl font-bold text-foreground tracking-tight",
  3: "text-lg font-bold text-foreground tracking-tight",
  4: "text-base font-semibold text-foreground tracking-tight",
  5: "text-sm font-semibold text-foreground tracking-tight",
  6: "text-xs font-semibold text-foreground tracking-tight",
};

const sizeStyles = {
  xs: "text-xs",
  sm: "text-sm",
  base: "text-base",
  lg: "text-lg",
  xl: "text-xl",
  "2xl": "text-2xl",
  "3xl": "text-3xl",
};

export const Heading: React.FC<HeadingProps> = ({
  level = 2,
  size,
  uppercase = false,
  children,
  className = "",
  ...props
}) => {
  const Component = `h${level}` as const;
  const sizeClass = size ? sizeStyles[size] : "";
  const uppercaseClass = uppercase ? "uppercase" : "";
  return (
    <Component
      className={`${headingStyles[level]} ${sizeClass} ${uppercaseClass} ${className}`.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {children}
    </Component>
  );
};

export interface TextProps extends React.HTMLAttributes<HTMLParagraphElement> {
  variant?: "body" | "muted" | "subtitle" | "caption" | "lead" | "default" | "subtle" | "primary";
  size?: "xs" | "sm" | "base" | "lg" | "xl" | "2xl";
  uppercase?: boolean;
  children: React.ReactNode;
}

const textStyles = {
  body: "text-sm text-foreground leading-relaxed",
  default: "text-foreground leading-relaxed",
  muted: "text-muted-foreground leading-relaxed",
  subtle: "text-muted-foreground/70 leading-relaxed",
  subtitle: "text-xs font-medium text-muted-foreground uppercase tracking-wider",
  caption: "text-xs text-muted-foreground",
  lead: "text-base text-muted-foreground leading-relaxed",
  primary: "text-primary font-bold",
};

export const Text: React.FC<TextProps> = ({
  variant = "body",
  size,
  uppercase = false,
  children,
  className = "",
  ...props
}) => {
  const sizeClass = size ? sizeStyles[size] : "";
  const uppercaseClass = uppercase ? "uppercase" : "";
  return (
    <p
      className={`${textStyles[variant]} ${sizeClass} ${uppercaseClass} ${className}`.replace(/\s+/g, ' ').trim()}
      {...props}
    >
      {children}
    </p>
  );
};
