import {
  ActionFunction,
  json,
  LoaderFunction,
  redirect,
} from "@remix-run/node";
import { getUserSession } from "~/util/session.server";
import {
  useFetcher,
  useLoaderData,
} from "@remix-run/react";
import Feed from "~/routes/timeline+/component/feed";
import { UserDto } from "~/types/UserDto";
import { useEffect, useState } from "react";
import Pagination from "~/routes/components/pagination";
import {PaginationResponse} from "~/types/PaginationResponse";

export interface LoaderData {
  profileUser?: UserDto;
  currentUser?: UserDto;
  followed?: boolean;
  pagination: PaginationResponse;
}

export interface ActionData {
  followed?: boolean;
}

export const loader: LoaderFunction = async ({ request, params }) => {
  const { username } = params;
  const url = new URL(request.url);
  const page = Number(url.searchParams.get("page") || "0");

  const session = await getUserSession(request);
  const token = session.get("token");

  if (!token) {
    return redirect("/login");
  }

  const messagesResponse = await fetch(
    `${process.env.API_BASE_URL}api/twit/user/${username}?page=${page}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );

  const userResponse = await fetch(`${process.env.API_BASE_URL}api/user`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });

  const profileUserResponse = await fetch(
    `${process.env.API_BASE_URL}api/user/${username}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );

  const followResponse = await fetch(
    `${process.env.API_BASE_URL}api/follow/${username}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );

  return json({
    profileUser: await profileUserResponse.json(),
    currentUser: await userResponse.json(),
    followed: await followResponse.json(),
    pagination: await messagesResponse.json(),
  });
};

export const action: ActionFunction = async ({ request, params }) => {
  const { username } = params;
  const session = await getUserSession(request);
  const token = session.get("token");

  if (!token) {
    return redirect("/login");
  }

  const formData = await request.formData();
  const intent = formData.get("intent");

  if (intent === "follow") {
    const response = await fetch(
      `${process.env.API_BASE_URL}api/follow/${username}`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      }
    );

    if (!response.ok) {
      return json({ error: "Failed to follow user" }, { status: 400 });
    }
    return json({ followed: true });
  } else if (intent === "unfollow") {
    const response = await fetch(
      `${process.env.API_BASE_URL}api/follow/${username}/unfollow`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
      }
    );

    if (!response.ok) {
      return json({ error: "Failed to unfollow user" }, { status: 400 });
    }

    return json({ followed: false });
  }

  return json({ error: "Invalid intent" }, { status: 400 });
};

export default function UserPage() {
  const { profileUser, currentUser, followed, pagination } =
    useLoaderData<LoaderData>();
  const [isFollowed, setIsFollowed] = useState(followed);
  const followFetcher = useFetcher<ActionData>();

  useEffect(() => {
    if (
      followFetcher.data &&
      typeof followFetcher.data.followed !== "undefined"
    ) {
      setIsFollowed(followFetcher.data.followed);
    }
  }, [followFetcher.data]);

  return (
    <div>
      <h2>User timeline</h2>
      {profileUser && (
        <div className="followstatus">
          {currentUser!.id === profileUser.id ? (
            <p>This is you!</p>
          ) : isFollowed ? (
            <>
              You are currently following this user.
              <followFetcher.Form method="post" style={{ display: "inline" }}>
                {/*TODO: Remove intent*/}
                <input type="hidden" name="intent" value="unfollow" />
                <button type="submit" className="unfollow">
                  Unfollow user
                </button>
              </followFetcher.Form>
              .
            </>
          ) : (
            <>
              You are not yet following this user.
              <followFetcher.Form method="post" style={{ display: "inline" }}>
                {/*TODO: Remove intent*/}
                <input type="hidden" name="intent" value="follow" />
                <button type="submit" className="follow">
                  Follow user
                </button>
              </followFetcher.Form>
              .
            </>
          )}
        </div>
      )}

      <Feed messages={pagination.data} />
      <Pagination currentPage={pagination.pagination.currentPage} totalPages={pagination.pagination.totalPages} />
    </div>
  );
}
