import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";
import Feed from "~/routes/timeline+/component/feed";
import { PaginationResponse } from "~/types/PaginationResponse";

export interface LoaderData {
  pagaination: PaginationResponse; 
}

export const loader: LoaderFunction = async () => {
  const response = await fetch(
    `${process.env.API_BASE_URL}api/twit/public?page=0`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    }
  );
  return json({
    pagaination: await response.json(),
  });
};

export default function PublicTimelinePage() {
  const { pagaination } = useLoaderData<LoaderData>();
  console.log(pagaination);
  return (
    <div>
      <h2>Public Timeline</h2>

      <Feed messages={pagaination.data} />

      <div>
        //logik for at lave buttons (prev og next), loop, hvor vi maks kan have x knapper
      </div>
    </div>
  );
}
