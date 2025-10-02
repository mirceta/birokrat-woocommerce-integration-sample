namespace common_ops.Pipelines
{
    public static class Constants
    {
        internal static readonly string SHUTDOWN_COMMAND = "--!--EXIT--!--";

        public static readonly string MESSAGE_SERVER_TIMEOUT = "Timeout waiting for message.";
        public static readonly string MESSAGE_SERVER_PIPE_CLOSED_BY_CLIENT = "Pipe closed by client.";
        public static readonly string MESSAGE_SERVER_CLIENT_DISCONECTED = "Client has disconnected.";

        public static readonly string MESSAGE_SERVER_WAITING_FOR_CLIENT_CONNECTION = "Waiting for client connection...";
        public static readonly string MESSAGE_SERVER_TIMEOUT_WAITING_FOR_CLIENT = "Timeout waiting for client.";
        public static readonly string MESSAGE_SERVER_CLIENT_CONNECTED = "A client has connected.";
        public static readonly string MESSAGE_SERVER_PIPE_SERVER_CLOSED = "Pipeline Server Closed";

        public static readonly string MESSAGE_CLIENT_NOT_CONNECTED = "Client not connected!";
        public static readonly string MESSAGE_CLIENT_SENDING_MESSAGE = "Sending message via Pipelines...";
        public static readonly string MESSAGE_CLIENT_CONNECTING = "Client connecting...";
        public static readonly string MESSAGE_CLIENT_CONNECTED = "Client connected.";

    }
}
