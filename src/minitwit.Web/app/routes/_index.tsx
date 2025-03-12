import {LoaderFunction, redirect} from "@remix-run/node";
import { getUserSession } from "~/util/session.server";
export const loader: LoaderFunction = async ({ request })=>  {
  const session = await getUserSession(request);
  const token = session.get("token");
  if (!token) {
    return redirect("/timeline/public");
  }
  return redirect("/timeline")
}

export default function Index() {

  return null;
}
