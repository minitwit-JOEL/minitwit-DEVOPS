export interface Message {
    id: number;
    authorId: number;
    message: string;
    date: string;
    flagged: boolean;

    // TODO: Delete this
    username: string;
}
