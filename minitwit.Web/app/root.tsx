// app/root.tsx
import {
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
  useLoaderData,
} from "@remix-run/react";
import type {
  LoaderFunction,
  MetaFunction,
} from "@remix-run/node";
import { json } from "@remix-run/node";
import "./styles/style.css";

interface LoaderData {
  user: { username: string } | null;
  flashes: string[];
}

export const loader: LoaderFunction = async () => {
  return json({
    user: null,
    flashes: [],
  });
};

export const meta: MetaFunction = () => {
  return [
    { title: "Welcome | MiniTwit" },
    { name: "viewport", content: "width=device-width, initial-scale=1" },
  ];
};

export default function App() {
  const { user, flashes } = useLoaderData<LoaderData>();

  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <Meta />
        <Links />
      </head>
      <body>
        <div className="page">
          <h1>MiniTwit</h1>
          <div className="navigation">
            {user ? (
              <>
                <a href="/timeline">my timeline</a> |{" "}
                <a href="/timeline">public timeline</a> |{" "}
                <a href="/logout">sign out [{user.username}]</a>
              </>
            ) : (
              <>
                <a href="/timeline">public timeline</a> |{" "}
                <a href="/register">sign up</a> | <a href="/login">sign in</a>
              </>
            )}
          </div>

          {flashes && flashes.length > 0 && (
            <ul className="flashes">
              {flashes.map((message, index) => (
                <li key={index}>{message}</li>
              ))}
            </ul>
          )}

          <div className="body">
            <Outlet />
          </div>

          <div className="footer">MiniTwit &mdash; A C# Application</div>
        </div>

        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}
