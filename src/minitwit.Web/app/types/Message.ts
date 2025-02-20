import { UserDto } from "~/types/UserDto";

export interface Message {
  id: number;
  authorId: number;
  author: UserDto | null;
  text: string;
  createdAt: string;
  flagged: boolean;
}
