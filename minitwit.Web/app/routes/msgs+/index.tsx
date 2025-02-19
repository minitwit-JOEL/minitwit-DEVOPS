import React, { useEffect, useState } from 'react';

interface Message {
  id: number;
  authorId: number;
  text: string;
  createdAt: string;
  flagged: boolean;
}

const Messages: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);

  useEffect(() => {
    const fetchMessages = async () => {
      try {
        const response = await fetch('/api/twit/public');
        if (!response.ok) {
          throw new Error('Network response was not ok');
        }
        const data = await response.json();
        setMessages(data);
      } catch (error) {
        console.error('Error fetching messages:', error);
      }
    };

    fetchMessages();
  }, []);

  return (
    <div>
      {messages.map((message) => (
        <div key={message.id}>
          <p>{message.text}</p>
          <small>{new Date(message.createdAt).toLocaleString()}</small>
        </div>
      ))}
    </div>
  );
};

export default Messages;
