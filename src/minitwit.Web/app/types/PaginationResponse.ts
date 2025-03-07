export interface PaginationData {
    pageSize: number;
    total: number;
    totalPages: number;
    currentPage: number;
}

export interface PaginationResponse {
    data: any[];
    pagination: PaginationData;
}