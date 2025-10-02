using common_ops.PipeLines;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.PipelineTests
{
    [TestFixture]
    public class SustainedServerTests
    {
        private readonly string PIPE_NAME = "PipeUnitTests";
        private List<string> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new List<string>();
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

            using (var server = new PipeServerBuilder().Build_Sustained(log, PIPE_NAME))
            {
                using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
                {
                    await Task.Delay(1000);
                }
            }

            Assert.That(_logger.Any(x => x.Contains("Client has Connected", StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task SendMessageAsync_Send3Messages_ReturnsSuccess()
        {
            Action<string> log = (string message) => _logger.Add(message);
            var message = "This is the message!";

            using (var server = new PipeServerBuilder().Build_Sustained(log, PIPE_NAME))
            {
                using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
                {
                    await client.SendMessageAsync(message);
                    await client.SendMessageAsync(message);
                    await client.SendMessageAsync(message);
                }
            }
            Assert.That(_logger.Where(x => x.Contains(message)).Count() == 3, Is.True);
        }

        //[Test]
        //public async Task SendMessageAsync_VeryLongMessage_ReturnsSuccess()
        //{
        //    Action<string> log = (string message) => _logger.Add(message);
        //    var message = GenerateRandomString(10000);
        //    using (var server = new PipeServerBuilder().Build_Sustained(log, PIPE_NAME))
        //    {
        //        using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 10000))
        //        {
        //            await client.SendMessageAsync(message);
        //        }
        //    }
        //    Assert.That(_logger.Any(x => x.Contains(message)), Is.True);
        //}

        //[Test]
        //public async Task SendMessageAsync_ClientTimeout_ReturnsSuccess()
        //{
        //    Action<string> log = (string message) => _logger.Add(message);
        //    var server = new PipeServerBuilder().Build_Sustained(log, PIPE_NAME);

        //    using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 2000))
        //    {

        //    }
        //    await Task.Delay(4000);

        //    Assert.That(_logger.Any(x => x.Contains("the pipe is being closed", StringComparison.OrdinalIgnoreCase)), Is.True);
        //    server.Dispose();
        //}

        //[Test]
        //public async Task SendMessageAsync_ClientTimeoutStartsNewServer_ReturnsSuccess()
        //{
        //    Action<string> log = (string message) => _logger.Add(message);
        //    var server = new PipeServerBuilder().Build_Sustained(log, PIPE_NAME);

        //    using (var client = new PipeClient(PIPE_NAME, (message) => _logger.Add(message), 2000))
        //    {

        //    }
        //    await Task.Delay(4000);

        //    Assert.That(_logger.Where(x => x.Contains("waiting for client connection", StringComparison.OrdinalIgnoreCase)).Count() == 2, Is.True);
        //    server.Dispose();
        //}
    }
}
