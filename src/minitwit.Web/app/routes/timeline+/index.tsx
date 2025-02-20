import {
  ActionFunction,
  json,
  LoaderFunction,
  redirect,
} from "@remix-run/node";
import { useLoaderData, Form } from "@remix-run/react";
import { UserDto } from "~/types/UserDto";
import { Message } from "~/types/Message";
import Feed from "~/routes/timeline+/component/feed";
import { getUserSession } from "~/util/session.server";

export interface LoaderData {
  currentUser?: UserDto;
  messagesResponse: messageReponse;
}

interface messageReponse {
  id: number;
  twits: Message[];
}

export const loader: LoaderFunction = async ({ request }) => {
  const session = await getUserSession(request);
  const token = session.get("token");

  const messagesResponse = await fetch(`${process.env.API_BASE_URL}api/twit/feed`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });

  const userResponse = await fetch(`${process.env.API_BASE_URL}api/user`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });

  return json({
    currentUser: userResponse.ok ? await userResponse.json() : null,
    messagesResponse: await messagesResponse.json(),
  });
};

export const action: ActionFunction = async ({ request }) => {
  const formData = await request.formData();
  const text = formData.get("text") as string;

  const session = await getUserSession(request);
  const token = session.get("token");

  if (!token) {
    return redirect("/login");
  }

  const response = await fetch(`${process.env.API_BASE_URL}api/twit`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(text),
  });

  if (!response.ok) {
    // cool
  }

  return json({
    status: 200,
    message: "Success",
  });
};

export default function Timeline() {
  const { currentUser, messagesResponse } = useLoaderData<LoaderData>();

  return (
    <div>
      <h2>My Timeline</h2>
      {currentUser && (
        <div className="twitbox">
          <h3>What is on your mind {currentUser.username}?</h3>
          <Form method="post">
            <p>
              <input type="text" name="text" size={60} />
              <input type="submit" value="Share" />
            </p>
          </Form>
        </div>
      )}

      <Feed messages={messagesResponse.twits} />
    </div>
  );
}
