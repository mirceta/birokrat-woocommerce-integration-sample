using common_ops.Pipelines;
using common_ops.PipeLines;
using common_ops.PipeLines.Server;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.PipelineTests
{
    [TestFixture]
    public class OneTimeServerTests
    {
        private readonly string PIPE_NAME = "PipeUnitTests";
        private List<string> _logger;
        private IPipeServer_Single _server;

        [SetUp]
        public void SetUp()
        {
            _logger = new List<string>();
        }

        [TearDown]
        public void Cleanup()
        {
            _server?.Dispose();
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Test]
        public async Task ConnectionTest_CanClientConnect_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);
            _server = new PipeServerBuilder().Build_OneTime(log, PIPE_NAME);

            using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
            {
                await Task.Delay(1000);
            }
            await _server.WaitForMessageAsync();

            Assert.That(_logger.Any(x => x.Contains(Constants.MESSAGE_SERVER_CLIENT_CONNECTED, StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task SendMessageAsync_ShortMessage_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);
            _server = new PipeServerBuilder().Build_OneTime(log, PIPE_NAME);
            var message = "This is the message!";

            using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
            {
                await client.SendMessageAsync(message);
            }
            await _server.WaitForMessageAsync();

            Assert.That(_logger.Any(x => x.Contains(message)), Is.True);
        }

        [Test]
        public async Task SendMessageAsync_LongMessage_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);
            _server = new PipeServerBuilder().Build_OneTime(log, PIPE_NAME);
            var message = GenerateRandomString(10000);

            using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
            {
                await client.SendMessageAsync(message);
            }
            await _server.WaitForMessageAsync();

            Assert.That(_logger.Any(x => x.Contains(message)), Is.True);
        }

        [Test]
        public async Task SendMessageAsync_ServerTimeout_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);
            _server = new PipeServerBuilder().Build_OneTime(log, PIPE_NAME, 3000);

            using var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 5000);
            await Task.Delay(4000);

            await _server.WaitForMessageAsync();

            var expected = _logger.Any(x => x.Contains(Constants.MESSAGE_SERVER_PIPE_SERVER_CLOSED, StringComparison.OrdinalIgnoreCase));
            Assert.That(expected, Is.True);
        }

        [Test]
        public async Task Connect_NoServerClientTimeout_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);

            try
            {
                using var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 3000);
                await Task.Delay(4000);
            }
            catch (TimeoutException ex)
            {
                Assert.Pass("Correct exception");
            }
            catch (Exception ex)
            {
                Assert.Fail("wrong exception message");
            }
        }
    }
}
