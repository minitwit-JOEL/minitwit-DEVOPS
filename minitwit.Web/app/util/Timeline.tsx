import { UserDto } from '~/types/UserDto';
import {Endpoint} from "~/routes/timeline/_index";

export function getTitle(endpoint: Endpoint, profileUser?: UserDto): string {
    switch (endpoint) {
        case Endpoint.PublicTimeline:
            return 'Public Timeline';
        case Endpoint.UserTimeline:
            return profileUser ? `${profileUser.username}'s Timeline` : 'User Timeline';
        case Endpoint.Timeline:
        default:
            return 'My Timeline';
    }
}
