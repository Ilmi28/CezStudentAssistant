import React from "react";
import Card from "./Card";
import { Heading, Text } from "./Typography";

export interface EmptyStateProps {
  title: string;
  description?: string;
  action?: React.ReactNode;
  className?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title,
  description,
  action,
  className = "",
}) => {
  return (
    <Card className={`p-8 sm:p-10 text-center shadow-xs flex flex-col items-center justify-center space-y-3 ${className}`}>
      <div className="max-w-md space-y-1.5 mx-auto">
        <Heading level={4} size="sm" className="font-bold text-foreground">
          {title}
        </Heading>
        {description && (
          <Text size="xs" variant="muted" className="leading-relaxed">
            {description}
          </Text>
        )}
      </div>

      {action && (
        <div className="pt-1">
          {action}
        </div>
      )}
    </Card>
  );
};

export default EmptyState;
