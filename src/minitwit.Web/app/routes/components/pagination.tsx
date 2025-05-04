import {Link} from "@remix-run/react";

interface PaginationProps {
    currentPage: number;
    totalPages: number;
}

export default function Pagination({currentPage, totalPages}: PaginationProps) {
    const prevPage = Math.max(0, currentPage - 1);
    const nextPage = Math.min(totalPages - 1, currentPage + 1);

    const maxButtons = 6;

    let startPage = Math.max(0, currentPage - Math.floor(maxButtons / 2));
    const endPage = Math.min(totalPages - 1, startPage + maxButtons - 1);

    if (endPage - startPage + 1 < maxButtons) {
        startPage = Math.max(0, endPage - maxButtons + 1);
    }

    return (
        <div style={{ display: "flex", justifyContent: "center", gap: "8px", marginTop: "20px" }}>
            <Link
                to={`?page=${prevPage}`}
                style={{
                    textDecoration: "none",
                    padding: "8px 12px",
                    border: "1px solid #ccc",
                    borderRadius: "5px",
                    backgroundColor: currentPage === 0 ? "#e0e0e0" : "#f4f4f4",
                    color: currentPage === 0 ? "#888" : "#333",
                    pointerEvents: currentPage === 0 ? "none" : "auto",
                }}
            >
                &laquo; Prev
            </Link>

            {Array.from({ length: endPage - startPage + 1 }, (_, index) => {
                const pageNumber = startPage + index;
                return (
                    <Link
                        key={pageNumber}
                        to={`?page=${pageNumber}`}
                        style={{
                            textDecoration: "none",
                            padding: "8px 12px",
                            border: "1px solid #ccc",
                            borderRadius: "5px",
                            backgroundColor: pageNumber === currentPage ? "#007bff" : "#f4f4f4",
                            color: pageNumber === currentPage ? "white" : "#333",
                            fontWeight: pageNumber === currentPage ? "bold" : "normal",
                        }}
                    >
                        {pageNumber + 1}
                    </Link>
                );
            })}

            <Link
                to={`?page=${nextPage}`}
                style={{
                    textDecoration: "none",
                    padding: "8px 12px",
                    border: "1px solid #ccc",
                    borderRadius: "5px",
                    backgroundColor: currentPage === totalPages - 1 ? "#e0e0e0" : "#f4f4f4",
                    color: currentPage === totalPages - 1 ? "#888" : "#333",
                    pointerEvents: currentPage === totalPages - 1 ? "none" : "auto",
                }}
            >
                Next &raquo;
            </Link>
        </div>
    )
}