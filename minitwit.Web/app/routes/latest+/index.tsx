// app/routes/latest/latest.tsx
import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";

type LoaderData = {
  latest: string;
};

export const loader: LoaderFunction = async () => {
  const response = await fetch("http://localhost:7168/api/twit/latest");
  const data: LoaderData = await response.json();
  return json(data);
};

export default function Latest() {
  const data = useLoaderData();
  return (
    <div>
      <h1>Latest Processed Command</h1>
    </div>
  );
}
