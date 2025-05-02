import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData, useSearchParams } from "@remix-run/react";
import Feed from "~/routes/timeline+/component/feed";
import { PaginationResponse } from "~/types/PaginationResponse";
import Pagination from "~/routes/components/pagination";

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

  return (
    <div>
      <h2>Public Timeline</h2>

      <Feed messages={pagination.data} />

      <Pagination currentPage={currentPage} totalPages={totalPages} />
    </div>
  );
}
