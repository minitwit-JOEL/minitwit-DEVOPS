import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";
import { Message } from "~/types/Message";
import Feed from "~/routes/timeline+/component/feed";

export interface LoaderData {
  messages: Message[];
}

export const loader: LoaderFunction = async () => {
  const messagesResponse = await fetch(
    `https://localhost:7168/api/twit/public`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  return json({
    messages: await messagesResponse.json(),
  });
};

export default function PublicTimelinePage() {
  const { messages } = useLoaderData<LoaderData>();

  return (
    <div>
      <h2>Public Timeline</h2>

      <Feed messages={messages} />
    </div>
  );
}
