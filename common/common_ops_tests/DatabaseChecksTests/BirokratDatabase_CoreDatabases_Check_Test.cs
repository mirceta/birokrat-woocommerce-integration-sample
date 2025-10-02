using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    public class BirokratDatabase_CoreDatabases_Check_Test
    {
        BirokratDatabase_CoreDatabases_Check _check;
        Mock<IBirokratQueryExecutor> _birokratQueryExecutorMock;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = "SQLSERVER";

            _birokratQueryExecutorMock = new Mock<IBirokratQueryExecutor>();

            _check = new BirokratDatabase_CoreDatabases_Check(_birokratQueryExecutorMock.Object, _connectionString);
        }

        [Test]
        public async Task Run_CoreDatabasesArePresent_ReturnsSuccess()
        {
            var requiredCoreDatabases = new List<string> { "application", "biromaster", "configuration" };

            _birokratQueryExecutorMock.Setup(x => x.GetCoreDatabasesAsync(_connectionString))
                .ReturnsAsync(requiredCoreDatabases);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.SupersetOf(requiredCoreDatabases).IgnoreCase);
        }

        [Test]
        public async Task Run_OneCoreDatabaseIsNotPresent_ReturnsFailure()
        {
            var requiredCoreDatabases = new List<string> { "biromaster", "configuration" };

            _birokratQueryExecutorMock.Setup(x => x.GetCoreDatabasesAsync(_connectionString))
                .ReturnsAsync(requiredCoreDatabases);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(item => item.Contains("NULL", StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_AllCoreDatabasesAreNotPresent_ReturnsFailure()
        {
            var requiredCoreDatabases = new List<string> { };

            _birokratQueryExecutorMock.Setup(x => x.GetCoreDatabasesAsync(_connectionString))
                .ReturnsAsync(requiredCoreDatabases);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Empty);
        }

        [Test]
        public async Task Run_ErrorDuringDatabaseRetrieval_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetCoreDatabasesAsync(_connectionString))
                .Throws(new Exception());

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }
    }
}
