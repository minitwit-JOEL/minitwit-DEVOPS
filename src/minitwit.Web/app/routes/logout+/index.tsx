import { LoaderFunction, redirect } from "@remix-run/node";
import { destroyUserSession, getUserSession } from "~/util/session.server";

export const loader: LoaderFunction = async ({ request }) => {
  const session = await getUserSession(request);
  const token = session.get("token");

  const response = await fetch(`${process.env.API_BASE_URL}api/auth/logout`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    return redirect("/");
  }


  return redirect("/login", {
    headers: { "Set-Cookie": await destroyUserSession(request) },
  });
};

export default function Logout() {
  return <></>;
}
