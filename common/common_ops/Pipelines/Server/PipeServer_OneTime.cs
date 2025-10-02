using common_ops.Pipelines;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace common_ops.PipeLines.Server
{
    internal class PipeServer_OneTime : IPipeServer_Single
    {
        private readonly Action<string> _logger;
        private readonly PipeServerUtils _pipeServerUtils;
        private readonly Task _pipeServiceTask;
        private TaskCompletionSource<bool> _taskCompletionSource;
        private NamedPipeServerStream _server;

        private bool _disposed = false;
        public bool IsActive => _taskCompletionSource.Task.IsCompleted;


        public PipeServer_OneTime(Action<string> logger, PipeServerUtils pipeServerUtils, int timeoutInMils)
        {
            _logger = logger;
            _pipeServerUtils = pipeServerUtils;

            _server = pipeServerUtils.CreateNewServer();

            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pipeServiceTask = Task.Run(async () => await Listen_OneTime(timeoutInMils));
        }

        private async Task Listen_OneTime(int timeoutInMils = 60000)
        {
            try
            {
                _logger?.Invoke(Constants.MESSAGE_SERVER_WAITING_FOR_CLIENT_CONNECTION);
                var connected = await _pipeServerUtils.WaitForConnectionAsyncWrapper(timeoutInMils, _server);
                if (!connected)
                {
                    _logger?.Invoke(Constants.MESSAGE_SERVER_TIMEOUT_WAITING_FOR_CLIENT);
                    return;
                }

                _logger?.Invoke(Constants.MESSAGE_SERVER_CLIENT_CONNECTED);

                using (var reader = new StreamReader(_server))
                {
                    await _pipeServerUtils.WaitForMessageAsync(reader, timeoutInMils);
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke("Exception: " + ex.Message);
                _taskCompletionSource.TrySetException(ex);
            }
            finally
            {
                _logger?.Invoke(Constants.MESSAGE_SERVER_PIPE_SERVER_CLOSED);

                try
                {
                    await _server?.FlushAsync();
                }
                catch { } //this is empty catch because if it fails server was alerady closed and is already logged. No need for two same messages

                _taskCompletionSource.TrySetResult(true);
            }
        }

        public async Task WaitForMessageAsync()
        {
            await _taskCompletionSource.Task;
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
                if (!_taskCompletionSource.Task.IsCompleted)
                    _taskCompletionSource.TrySetResult(true);

                try
                {
                    if (_pipeServiceTask?.IsCompleted ?? false)
                        _pipeServiceTask.Dispose();
                    // else skip to avoid InvalidOperationException
                }
                catch (Exception ex)
                {
                    _logger?.Invoke("Error disposing task: " + ex.Message);
                }

                try { _server?.Dispose(); } catch { }
            }
            _disposed = true;
        }

        ~PipeServer_OneTime()
        {
            Dispose(false);
        }
    }
}
