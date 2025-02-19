// Messages.tsx
import React, { useEffect, useState } from 'react';

interface Message {
 content: string;
 pub_date: string;
 user: string;
}

const Messages: React.FC = () => {
 const [messages, setMessages] = useState<Message[]>([]);
 const [error, setError] = useState<string | null>(null);

 useEffect(() => {
   const fetchMessages = async () => {
     try {
       const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/twit/msgs`);
       if (!response.ok) {
         throw new Error('Failed to fetch messages');
       }
       const data = await response.json();
       setMessages(data);
     } catch (err) {
       setError(err instanceof Error ? err.message : 'An unexpected error occurred');
     }
   };

   fetchMessages();
 }, []);

 if (error) {
   return <div>Error: {error}</div>;
 }

 return (
   <div>
     {messages.length > 0 ? (
       <ul>
         {messages.map((msg, index) => (
           <li key={index}>
             <strong>{msg.user}</strong> ({msg.pub_date}): {msg.content}
           </li>
         ))}
       </ul>
     ) : (
       <p>No messages available.</p>
     )}
   </div>
 );
};

export default Messages;
