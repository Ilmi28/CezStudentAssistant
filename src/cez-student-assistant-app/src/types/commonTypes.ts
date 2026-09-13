export interface PagedResultDto<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedQueryParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  courseId?: string;
}
