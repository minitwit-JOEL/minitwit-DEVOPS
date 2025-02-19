// MessagesPage.tsx
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

const MessagesPage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const [messages, setMessages] = useState<any[]>([]);

  useEffect(() => {
    const fetchMessages = async () => {
      try {
        const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/twit/msgs${username}`);
        if (!response.ok) {
          throw new Error('Failed to fetch messages');
        }
        const data = await response.json();
        setMessages(data);
      } catch (error) {
        console.error(error);
      }
    };

    if (username) {
      fetchMessages();
    }
  }, [username]);

  return (
    <div>
      <h1>Messages for {username}</h1>
      <ul>
        {messages.map((msg, index) => (
          <li key={index}>
            <strong>{msg.user}:</strong> {msg.content} <em>({new Date(msg.pubDate).toLocaleString()})</em>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default MessagesPage;
