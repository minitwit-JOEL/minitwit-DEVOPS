import { json, LoaderFunction } from '@remix-run/node';
import { useLoaderData, Form, Link } from '@remix-run/react';
import { UserDto } from "~/types/UserDto";
import {Message} from "~/types/Message";
import {getTitle} from "~/util/Timeline";

export enum Endpoint {
  PublicTimeline = 'public_timeline',
  UserTimeline = 'user_timeline',
  Timeline = 'timeline'
}

export interface LoaderData {
  endpoint: Endpoint;
  profileUser?: UserDto;
  currentUser?: UserDto;
  followed?: boolean;
  messages: Message[];
}

export const loader: LoaderFunction = async ({ request }) => {
  const url = new URL(request.url);
  const endpointParam = url.searchParams.get('endpoint');

  const endpoint: Endpoint =
      (endpointParam as Endpoint) || Endpoint.PublicTimeline;

  const currentUser: UserDto = {
    id: 1,
    username: 'currentuser',
    email: 'current@example.com'
  };

  const messages: Message[] = [
    {
      id: 1,
      authorId: 1,
      message: 'Hello world!',
      username: 'currentuser',
      date: new Date().toISOString(),
      flagged: false
    },
    {
      id: 2,
      authorId: 2,
      message: 'This is my message.',
      username: 'johnDoe',
      date: new Date().toISOString(),
      flagged: false
    }
  ];

  return json({ endpoint, profileUser: currentUser, currentUser, followed: false, messages });
};

export default function Timeline() {
  const { endpoint, profileUser, currentUser, followed, messages} = useLoaderData<LoaderData>();
  const title = getTitle(endpoint, profileUser!);

  return (
      <div>
        <h2>{title}</h2>
        {currentUser && (
            <>
              {endpoint === Endpoint.UserTimeline && profileUser && (
                  <div className="followstatus">
                    {currentUser.id === profileUser.id ? (
                        <p>This is you!</p>
                    ) : followed ? (
                        <p>
                          You are currently following this user.
                          <Link className="unfollow" to={`/unfollow/${profileUser.username}`}>
                            Unfollow user
                          </Link>
                          .
                        </p>
                    ) : (
                        <p>
                          You are not yet following this user.
                          <Link className="follow" to={`/follow/${profileUser.username}`}>
                            Follow user
                          </Link>
                          .
                        </p>
                    )}
                  </div>
              )}

              {endpoint === Endpoint.Timeline && (
                  <div className="twitbox">
                    <h3>What is on your mind {currentUser.username}?</h3>
                    <Form method="post">
                      <p>
                        <input type="text" name="text" size={60} />
                        <input type="submit" value="Share" />
                      </p>
                    </Form>
                  </div>
              )}
            </>
        )}

        <ul className="messages">
          {messages.length > 0 ? (
              messages.map((message, index) => (
                  <li key={index}>
                    <p>
                      <strong>
                        <Link to={`/user/${message.username}`}>{message.username}</Link>
                      </strong>{' '}
                      {message.message}
                      <br />
                      <small>&mdash; {message.date}</small>
                    </p>
                  </li>
              ))
          ) : (
              <li>
                <em>There is no message so far.</em>
              </li>
          )}
        </ul>
      </div>
  );
}
