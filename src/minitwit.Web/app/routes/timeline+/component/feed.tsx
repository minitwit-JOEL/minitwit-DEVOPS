import { gravatarUrl } from "~/util/Gravatar";
import { Link } from "@remix-run/react";
import { formatDatetime } from "~/util/FormatDate";
import { Message } from "~/types/Message";
import { UserDto } from "~/types/UserDto";

interface Props {
  messages: Message[];
}

export default function Feed({ messages }: Props) {
  return (
    <ul className="messages">
      {messages.length > 0 ? (
        messages.map((message, index) => {
          const author: UserDto | null = message.author ?? null;
          const email: string = author?.email ?? "unknown@example.com";
          const username: string = author?.username ?? "Unknown";

          return (
            <li key={index}>
              <img src={gravatarUrl(email, 48)} alt="User avatar" />
              <p>
                <strong>
                  {author ? (
                    <Link to={`/user/${username}`}>{username}</Link>
                  ) : (
                    username
                  )}
                </strong>{" "}
                {message.text}
                <br />
                <small>&mdash; {formatDatetime(message.createdAt)}</small>
              </p>
            </li>
          );
        })
      ) : (
        <li>
          <p>There is no message so far.</p>
        </li>
      )}
    </ul>
  );
}
