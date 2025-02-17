// app/routes/latest/latest.tsx
import { json, LoaderFunction } from "@remix-run/node";
import { useLoaderData } from "@remix-run/react";

// Define the type for the loader data
type LoaderData = {
  latest: string;  // Assuming `latest` is a string from the backend
};

// Define the loader function
export const loader: LoaderFunction = async () => {
  try {
    // Make sure the fetch URL is correct and the server is running
    const response = await fetch("http://localhost:7168/api/twit/latest");

    // If the response is not ok (status not in the range 200-299), throw an error
    if (!response.ok) {
      throw new Error(`Failed to fetch latest command. Status: ${response.status}`);
    }

    // Parse the JSON response
    const data: LoaderData = await response.json();
    
    // Return the parsed data as a Remix JSON response
    return json(data);
  } catch (error) {
    // Handle errors by logging them and returning a fallback value
    console.error("Error fetching latest processed command:", error);
    return json({ latest: "Error fetching data" });
  }
};

// The React component that renders the data
export default function Latest() {
  // Use the loader data
  const data = useLoaderData<LoaderData>();

  return (
    <div>
      <h1>Latest Processed Command</h1>
      <p>{data.latest}</p> {/* Display the latest value here */}
    </div>
  );
}
