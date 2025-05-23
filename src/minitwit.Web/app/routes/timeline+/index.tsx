import {
  ActionFunction,
  json,
  LoaderFunction,
  redirect,
} from "@remix-run/node";
import { useLoaderData, Form } from "@remix-run/react";
import { UserDto } from "~/types/UserDto";
import Feed from "~/routes/timeline+/component/feed";
import { getUserSession } from "~/util/session.server";
import Pagination from "~/routes/components/pagination";
import {PaginationResponse} from "~/types/PaginationResponse";

export interface LoaderData {
  currentUser?: UserDto;
  pagination: PaginationResponse;
}

export const loader: LoaderFunction = async ({ request }) => {
  const session = await getUserSession(request);
  const token = session.get("token");

  if (!token) {
    return redirect("/login");
  }

  const url = new URL(request.url);
  const page = Number(url.searchParams.get("page") || "0");

  const messagesResponse = await fetch(`${process.env.API_BASE_URL}api/twit/feed?page=${page}`, {
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
    pagination: await messagesResponse.json(),
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
  const { currentUser, pagination } = useLoaderData<LoaderData>();

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

      <Feed messages={pagination.data} />

      <Pagination currentPage={pagination.pagination.currentPage} totalPages={pagination.pagination.totalPages} />
    </div>
  );
}
