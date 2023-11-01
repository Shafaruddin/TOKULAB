
    export interface Conversation {
        id: string;
    }

    export interface User {
        name: string;
        screenName: string;
    }

    export interface Type {
        value: string;
        user: User;
    }

    export interface Sender {
        type: string;
    }

    export interface CustomerAssignedEvent {
        conversation: Conversation;
        type: Type;
        content: string;
        sender: Sender;
        timeStamp: number;
        id: string;
    }

    export interface GetCustomerByConverationIdOutputDto{
        customerName:string|null
        nric:string|null
        uen:string|null
    }

