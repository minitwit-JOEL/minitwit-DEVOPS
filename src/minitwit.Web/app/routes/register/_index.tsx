import type { ActionFunction, MetaFunction } from "@remix-run/node";
import { json, redirect } from "@remix-run/node";
import { Form, useActionData } from "@remix-run/react";
import { RegisterRequestDto } from "~/types/RegisterRequestDto";

export const meta: MetaFunction = () => {
  return [{ title: "Sign Up | MiniTwit" }];
};

interface ActionData {
  message?: string;
  username?: string;
  email?: string;
}

export const action: ActionFunction = async ({ request }) => {
  const formData = await request.formData();
  const username = formData.get("username") as string;
  const email = formData.get("email") as string;
  const password = formData.get("password") as string;
  const passwordRepeat = formData.get("passwordRepeat") as string;

  // Log the form data for debugging purposes
  console.log("Form Data:", { username, email, password, passwordRepeat });

  const registerRequestDto: RegisterRequestDto = {
    username: username,
    email: email,
    password: password,
    confirmPassword: passwordRepeat,
  };

  if (!username || !email || !password || !passwordRepeat) {
    console.log("Form validation failed: Missing fields");
    return json({ message: "Please fill in all fields" });
  }

  if (password !== passwordRepeat) {
    console.log("Password mismatch:", { password, passwordRepeat });
    return json({
      message: "Passwords do not match.",
      username: username,
      email: email,
    });
  }

  console.log("Sending data to API:", registerRequestDto);

  const response = await fetch(`${process.env.API_BASE_URL}api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(registerRequestDto),
  });


  if (!response.ok) {
    const responseData = await response.json();
    if (responseData?.error_msg === "The username is already taken") {
      return json({
        message: "Username is already taken. Please choose another one.",
        username: username,
        email: email,
      });
    }

    console.error("Registration failed:", responseData);
    return json({
      message: `Registration failed: ${responseData?.message ?? 'Unknown error'}`,
    });
  }


  console.log("Registration successful");
  return redirect("/login");
};


export default function SignUp() {
  const actionData = useActionData<ActionData>();

  return (
    <div className="signup-container">
      <h2>Sign Up</h2>
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
              defaultValue={actionData?.username || ""}
            />
          </dd>
          <dt>E-Mail:</dt>
          <dd>
            <input
              type="text"
              name="email"
              size={30}
              defaultValue={actionData?.email || ""}
            />
          </dd>
          <dt>Password:</dt>
          <dd>
            <input type="password" name="password" size={30} />
          </dd>
          <dt>
            Password <small>(repeat)</small>:
          </dt>
          <dd>
            <input type="password" name="passwordRepeat" size={30} />
          </dd>
        </dl>
        <div className="actions">
          <input type="submit" value="Sign Up" />
        </div>
      </Form>
    </div>
  );
}
