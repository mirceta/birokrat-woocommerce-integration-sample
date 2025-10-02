using common_ops.PipeLines.Server;
using System;
using System.Threading;

namespace common_ops.PipeLines
{
    public class PipeServerBuilder
    {
        /// <summary>
        /// Listens for a single client connection and handles incoming messages. If <paramref name="timeoutInMils"/> is not -1 
        /// (Timeout.Infinite), the server waits for a connection until the timeout is reached. If a client connects within the timeout, 
        /// messages are processed until the client disconnects. If no client connects within the timeout, the server logs a timeout 
        /// message and exits the listen operation. Does not repeat listening after timeout.
        /// <para>Intended for use with a corresponding <see cref="PipeClient"/> on the client side.</para>
        /// </summary>
        /// <param name="timeoutInMils">The timeout in milliseconds for waiting for a client connection.</param>
        /// <returns>A task representing the asynchronous server listening and message handling operation.</returns>
        public IPipeServer_Single Build_OneTime(Action<string> logger, string pipeName, int timeoutInMils = Timeout.Infinite)
        {
            var utilities = new PipeServerUtils(logger, pipeName);
            var pipe = new PipeServer_OneTime(logger, utilities, timeoutInMils);
            return pipe;
        }

        /// <summary>
        /// Listens for client connections and handles incoming messages. If parameters are default server will indefinitely wait 
        /// for connection. If <paramref name="timeoutInMils"/> is not -1 (Timeout.Infinite), it will wait for connection, disconnect 
        /// on timeout and repeat. Server will be closed after <paramref name="retryLimit"/> is reached and no connection is established.
        /// <para>Intended for use with a corresponding <see cref="PipeClient"/> on the client side.</para>
        /// </summary>
        /// <param name="timeoutInMils">The timeout in milliseconds for waiting for a client connection.</param>
        /// <param name="retryLimit">The amount of retries when trying to establish connection</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public IPipeServer_Sustained Build_Sustained(Action<string> logger, string pipeName, int timeoutInMils = Timeout.Infinite, uint retryLimit = 3)
        {
            var utilities = new PipeServerUtils(logger, pipeName);
            var pipe = new PipeServer_Sustained(logger, utilities, retryLimit, timeoutInMils);
            return pipe;
        }
    }
}
