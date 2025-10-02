using common_ops.Pipelines;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace common_ops.PipeLines.Server
{
    public class PipeServerUtils
    {
        private readonly Action<string> _logger;
        private readonly string _pipeName;

        public PipeServerUtils(Action<string> logger, string pipeName)
        {
            _logger = logger;
            _pipeName = pipeName;
        }

        internal NamedPipeServerStream CreateNewServer()
        {
            try
            {
                var server = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                _logger?.Invoke($"PipeServer '{_pipeName}' initialized");
                return server;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"ERROR: {ex}");
                throw;
            }
        }

        internal async Task WaitForMessageAsync(StreamReader reader, int timeoutInMills)
        {
            while (true)
            {
                var readTask = reader.ReadLineAsync();
                var timeoutTask = Task.Delay(timeoutInMills);

                var completed = await Task.WhenAny(readTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    _logger?.Invoke(Constants.MESSAGE_SERVER_TIMEOUT);
                    return;
                }

                string message = await readTask;

                if (message == null)
                {
                    _logger?.Invoke(Constants.MESSAGE_SERVER_PIPE_CLOSED_BY_CLIENT);
                    return;
                }

                if (message?.IndexOf(Constants.SHUTDOWN_COMMAND, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _logger?.Invoke(Constants.MESSAGE_SERVER_CLIENT_DISCONECTED);
                    return;  // Exit the reading loop 
                }

                _logger?.Invoke("MESSAGE: " + message);
            }
        }

        internal async Task<NamedPipeServerStream> ResetConnection(NamedPipeServerStream server)
        {
            if (server.IsConnected)
                server.Disconnect();

            server.Close();
            server.Dispose();

            await Task.Delay(100);

            return CreateNewServer(); // Recreate the server for the next client
        }

        internal async Task<bool> WaitForConnectionAsyncWrapper(int timeoutInMils, NamedPipeServerStream server)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(timeoutInMils))
            {
                try
                {
                    await server.WaitForConnectionAsync(cts.Token);
                    return server.IsConnected;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
        }
    }
}
