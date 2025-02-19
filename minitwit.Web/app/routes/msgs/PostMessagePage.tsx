// PostMessagePage.tsx
import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

const PostMessagePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const [content, setContent] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    try {
      const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/twit/msgs${username}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ content }),
      });
      if (response.ok) {
        navigate(`/user/${username}`);
      } else {
        throw new Error('Failed to post message');
      }
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <div>
      <h1>Post a Message for {username}</h1>
      <form onSubmit={handleSubmit}>
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="Enter your message"
          required
        />
        <button type="submit">Post Message</button>
      </form>
    </div>
  );
};

export default PostMessagePage;
