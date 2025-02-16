import { createCookieSessionStorage } from "@remix-run/node";
import { UserDto } from "~/types/UserDto";

const sessionStorage = createCookieSessionStorage({
  cookie: {
    name: "__session",
    secure: process.env.NODE_ENV === "production",
    secrets: ["your-secret-key"],
    sameSite: "lax",
    path: "/",
    httpOnly: true,
  },
});

export const getUserSession = async (request: Request) => {
  return await sessionStorage.getSession(request.headers.get("Cookie"));
};

export const getCurrentUser = async (
  request: Request
): Promise<{ user: UserDto; token: string }> => {
  const session = await getUserSession(request);
  return {
    user: session.get("user") || null,
    token: session.get("token") || null,
  };
};

export const createUserSession = async (token: string, user: UserDto) => {
  const session = await sessionStorage.getSession();
  session.set("token", token);
  session.set("user", user);

  return await sessionStorage.commitSession(session);
};

export const destroyUserSession = async (request: Request) => {
  const session = await getUserSession(request);
  return await sessionStorage.destroySession(session);
};
