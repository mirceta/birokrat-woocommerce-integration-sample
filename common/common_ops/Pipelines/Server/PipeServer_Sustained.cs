using common_ops.Pipelines;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace common_ops.PipeLines.Server
{
    internal class PipeServer_Sustained : IPipeServer_Sustained
    {
        private readonly Action<string> _logger;
        private readonly PipeServerUtils _pipeServerUtils;
        private readonly Task _pipeServiceTask;
        private TaskCompletionSource<bool> _taskCompletionSource;
        private NamedPipeServerStream _server;

        public bool IsActive => !_taskCompletionSource.Task.IsCompleted;

        public PipeServer_Sustained(Action<string> logger, PipeServerUtils pipeServerUtils, uint retryLimit, int timeoutInMils)
        {
            _logger = logger;
            _pipeServerUtils = pipeServerUtils;

            _server = pipeServerUtils.CreateNewServer();

            _taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pipeServiceTask = Task.Run(async () => await Listen(retryLimit, timeoutInMils).ConfigureAwait(false));
        }

        private async Task Listen(uint retryLimit = 3, int timeoutInMils = Timeout.Infinite)
        {
            uint local_retry_limit = retryLimit;

            while (!_taskCompletionSource.Task.IsCompleted)
            {
                try
                {
                    _logger?.Invoke(Constants.MESSAGE_SERVER_WAITING_FOR_CLIENT_CONNECTION);
                    var connected = await _pipeServerUtils.WaitForConnectionAsyncWrapper(timeoutInMils, _server);
                    if (!connected)
                    {
                        _logger?.Invoke(Constants.MESSAGE_SERVER_TIMEOUT_WAITING_FOR_CLIENT);
                        if (--local_retry_limit > 0)
                            continue;
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        local_retry_limit = retryLimit; //This will reset the counter. So each new connection has 3 time fail limit
                    }
                    _logger?.Invoke(Constants.MESSAGE_SERVER_CLIENT_CONNECTED);

                    using (var reader = new StreamReader(_server))
                    {
                        await _pipeServerUtils.WaitForMessageAsync(reader, timeoutInMils).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Invoke("Server error: " + ex.Message);
                }
                finally
                {
                    await _server.FlushAsync();
                    if (!_taskCompletionSource.Task.IsCompleted)
                        _server = await _pipeServerUtils.ResetConnection(_server);
                }
            }
        }

        public void Dispose()
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

            _logger?.Invoke(Constants.MESSAGE_SERVER_PIPE_SERVER_CLOSED);
        }
    }
}
