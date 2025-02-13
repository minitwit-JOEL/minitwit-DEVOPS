import type { ActionFunction, MetaFunction } from "@remix-run/node";
import { json, redirect } from "@remix-run/node";
import { Form, useActionData } from "@remix-run/react";
import { LoginRequestDto } from "~/types/LoginRequestDto";

export const meta: MetaFunction = () => {
  return [{ title: "Sign In | MiniTwit" }];
};

interface ActionData {
  message?: string;
  username?: string;
}

export const action: ActionFunction = async ({ request }) => {
  const formData = await request.formData();
  const username = formData.get("username") as string;
  const password = formData.get("password") as string;

  const loginRequestDto: LoginRequestDto = {
    username: username,
    password: password,
  };

  if (!username || !password) {
    return json({
      message: "Login failed. Please fill in " + "both a username and password",
    });
  }

  const response = await fetch("https://localhost:7168/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(loginRequestDto),
  });

  if (!response.ok) {
    const responseData = await response.json();
    throw new Error(responseData.detail);
  }

  return redirect("/timeline");
};

export default function SignInPage() {
  const actionData = useActionData<ActionData>();

  return (
    <div>
      <h2>Sign In</h2>
      {actionData?.message && (
        <div className="error">
          <strong>Error:</strong> {actionData.message}
        </div>
      )}
      <Form method="post">
        <dl>
          <dt>Username:</dt>
          <dd>
            <input
              type="text"
              name="username"
              size={30}
              defaultValue={actionData?.username ?? ""}
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
