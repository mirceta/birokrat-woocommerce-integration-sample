using common_ops.Pipelines;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace common_ops.PipeLines
{
    /// <summary>
    /// Manages client-side operations for named pipe communication.
    /// </summary>
    public class PipeClient : IDisposable
    {
        private bool _disposed = false;
        private readonly NamedPipeClientStream _client;
        private readonly Action<string> _logger;
        private readonly StreamWriter _writer;

        /// <summary>
        /// Initializes a new instance of the PipeClient class, connects to the server,
        /// and prepares for sending messages.
        /// </summary>
        /// <param name="pipeName">The name of the pipe to connect to.</param>
        /// <param name="logger">An action delegate to log messages.</param>
        /// <param name="timeoutInMils">The timeout in milliseconds for connecting to the server.</param>
        /// <remarks>
        /// This constructor attempts to connect to the named pipe server and initializes a StreamWriter
        /// for sending messages if the connection is successful. 
        /// <para><c>!!! The class should be used within an 'using' statement to ensure proper disposal of resources 
        /// or call Dispose() manually !!!</c>></para>
        /// </remarks>
        public PipeClient(string pipeName, Action<string> logger, int timeoutInMils = 5000)
        {
            _logger = logger;
            _client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);

            CreateClientAndConnectToServer(_client, _logger, timeoutInMils);

            if (_client.IsConnected)
                _writer = new StreamWriter(_client) { AutoFlush = true };
        }

        private void CreateClientAndConnectToServer(NamedPipeClientStream client, Action<string> logger, int timeoutInMils)
        {
            try
            {
                logger?.Invoke(Constants.MESSAGE_CLIENT_CONNECTING);
                client.Connect(timeoutInMils);
                logger?.Invoke(Constants.MESSAGE_CLIENT_CONNECTED);
            }
            catch (TimeoutException ex)
            {
                logger?.Invoke("Error: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sends a message to the connected pipe server.
        /// </summary>
        /// <param name="message">The message to send to the server.</param>
        /// <remarks>
        /// This method writes a message to the server using a StreamWriter. If the client is not connected,
        /// a log message is generated. It handles any exceptions that might occur during the message sending process.
        /// </remarks>
        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (_writer == null)
                {
                    _logger?.Invoke(Constants.MESSAGE_CLIENT_NOT_CONNECTED);
                    return;
                }
                _logger?.Invoke(Constants.MESSAGE_CLIENT_SENDING_MESSAGE);
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger?.Invoke("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Releases all resources used by the PipeClient.
        /// </summary>
        /// <remarks>
        /// This method disposes of the StreamWriter and the NamedPipeClientStream, ensuring no resource leaks.
        /// Always use this class within a 'using' statement to guarantee that resources are properly cleaned up.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running
        }

        // Protected virtual dispose method for subclasses
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    _writer?.WriteLine(Constants.SHUTDOWN_COMMAND);
                    _writer?.Flush();
                    Thread.Sleep(50);
                }
                catch { }

                try
                {
                    _writer?.Dispose();
                }
                catch { }
            }
            _disposed = true;
        }

        ~PipeClient()
        {
            Dispose(false);
        }
    }
}

