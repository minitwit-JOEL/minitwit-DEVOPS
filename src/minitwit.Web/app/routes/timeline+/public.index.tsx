import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData, Link, useSearchParams } from "@remix-run/react";
import Feed from "~/routes/timeline+/component/feed";
import { PaginationResponse } from "~/types/PaginationResponse";

export interface LoaderData {
  pagination: PaginationResponse;
}

export const loader: LoaderFunction = async ({ request }) => {
  const url = new URL(request.url);
  const page = Number(url.searchParams.get("page") || "0");  

  const response = await fetch(
    `${process.env.API_BASE_URL}api/twit/public?page=${page}`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  return json({
    pagination: await response.json(),
  });
};

export default function PublicTimelinePage() {
  const { pagination } = useLoaderData<LoaderData>();
  const [searchParams] = useSearchParams();
  const currentPage = Number(searchParams.get("page") || "0");  
  const totalPages = pagination.pagination.totalPages;
  const prevPage = Math.max(0, currentPage - 1); 
  const nextPage = Math.min(totalPages - 1, currentPage + 1); 
  const maxButtons = 6;

  let startPage = Math.max(0, currentPage - Math.floor(maxButtons / 2));
  const endPage = Math.min(totalPages - 1, startPage + maxButtons - 1);

  if (endPage - startPage + 1 < maxButtons) {
    startPage = Math.max(0, endPage - maxButtons + 1);
  }

  return (
    <div>
      <h2>Public Timeline</h2>

      <Feed messages={pagination.data} />

      {/* Pagination Section */}
      <div style={{ display: "flex", justifyContent: "center", gap: "8px", marginTop: "20px" }}>
        {/* Prev Button */}
        <Link
          to={`/timeline/public?page=${prevPage}`}
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

        {/* Page Buttons */}
        {Array.from({ length: endPage - startPage + 1 }, (_, index) => {
          const pageNumber = startPage + index;
          return (
            <Link
              key={pageNumber}
              to={`/timeline/public?page=${pageNumber}`}
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

        {/* Next Button */}
        <Link
          to={`/timeline/public?page=${nextPage}`}
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
    </div>
  );
}
