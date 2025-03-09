import { Message } from "~/types/Message";

export interface PaginationData {
    pageSize: number;
    total: number;
    totalPages: number;
    currentPage: number;
}

export interface PaginationResponse {
    data: Message[];
    pagination: PaginationData;
}