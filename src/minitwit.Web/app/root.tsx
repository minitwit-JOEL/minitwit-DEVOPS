import {
  Link,
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useLoaderData,
} from "@remix-run/react";
import type { LoaderFunction, MetaFunction } from "@remix-run/node";
import { json } from "@remix-run/node";
import { AuthProvider, useAuth } from "~/context/AuthContext";
import "./styles/style.css";
import { getCurrentUser } from "~/util/session.server";
import { UserDto } from "~/types/UserDto";

export const loader: LoaderFunction = async ({ request }) => {
  const { user } = await getCurrentUser(request);
  return json({ user });
};

export const meta: MetaFunction = () => {
  return [
    { title: "Welcome | MiniTwit" },
    { name: "viewport", content: "width=device-width, initial-scale=1" },
  ];
};

export default function App() {
  const { user } = useLoaderData<{ user: UserDto }>();

  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <Meta />
        <Links />
      </head>
      <body>
        <AuthProvider user={user}>
          <PageLayout />
          <ScrollRestoration />
          <Scripts />
        </AuthProvider>
      </body>
    </html>
  );
}

function PageLayout() {
  const { isAuthenticated } = useAuth();

    const handleLogout = async (e: React.MouseEvent<HTMLAnchorElement, MouseEvent>) => {
        e.preventDefault();

        await fetch("/logout", { method: "GET" });

        window.location.href = "/login";
    };

  return (
    <div className="page">
      <h1>MiniTwit</h1>
      <div className="navigation">
        {isAuthenticated ? (
          <div>
            <Link to={"/timeline"}>my timeline</Link> |{" "}
            <Link to={"/timeline/public"}>public timeline</Link> |{" "}
            <Link to={"/latest"}>Latest Processed Command</Link> |{" "}
            <Link to={"/logout"} onClick={handleLogout}>sign out</Link>
          </div>
        ) : (
          <div>
            <Link to={"/timeline/public"}>public timeline</Link> |{" "}
            <Link to={"/register"}>sign up</Link> |{" "}
            <Link to={"/latest"}>Latest Processed Command</Link> |{" "}
            <Link to={"/login"}>sign in</Link>
          </div>
        )}
      </div>

      <div className="body">
        <Outlet />
      </div>

      <div className="footer">MiniTwit &mdash; A C# Application</div>
    </div>
  );
}
