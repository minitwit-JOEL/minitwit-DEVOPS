import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";

interface LoaderData {
  latest: number;
}

export const loader: LoaderFunction = async () => {
  const response = await fetch("https://localhost:7168/api/twit/latest", {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    throw new Error("Failed to fetch the latest processed command ID");
  }

  const data = await response.json();
  return json<LoaderData>(data);
};

export default function LatestProcessedCommand() {
  const { latest } = useLoaderData<LoaderData>();

  return (
    <div>
      <h2>Latest Processed Command ID</h2>
      <p>{latest}</p>
    </div>
  );
}
