import {
  ActionFunction,
  json,
  LoaderFunction,
  MetaFunction,
} from "@remix-run/node";
import { Form, redirect } from "@remix-run/react";
import { createUserSession, getUserSession } from "~/util/session.server";

export const meta: MetaFunction = () => {
  return [{ title: "Sign In | MiniTwit" }];
};

interface LoginDto {
  id: number;
  username: string;
  email: string;
  token: string;
}

export const loader: LoaderFunction = async ({ request }) => {
  const session = await getUserSession(request);
  const token = session.get("token");

  if (token) {
    return redirect("/timeline");
  }
  return json({});
};

export const action: ActionFunction = async ({ request }) => {
  const formData = await request.formData();
  const username = formData.get("username") as string;
  const password = formData.get("password") as string;

  const response = await fetch(`${process.env.API_BASE_URL}api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });

  if (!response.ok) {
    return { message: "Invalid credentials", username };
  }

  const user: LoginDto = await response.json();

  return redirect("/", {
    headers: { "Set-Cookie": await createUserSession(user.token, user) },
  });
};

export default function SignInPage() {
  return (
    <div>
      <h2>Sign In</h2>
      <Form method="post">
        <dl>
          <dt>Username:</dt>
          <dd>
            <input
              type="text"
              name="username"
              size={30}
              placeholder={`Username`}
            />
          </dd>
          <dt>Password:</dt>
          <dd>
            <input type="password" name="password" size={30} />
          </dd>
        </dl>
        <div className="actions">
          <input type="submit" value="Sign In" />
        </div>
      </Form>
    </div>
  );
}
