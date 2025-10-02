using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class BirokratDatabase_YearcodeDatabases_Check_Test
    {
        BirokratDatabase_YearcodeDatabases_Check _check;
        Mock<IBirokratQueryExecutor> _birokratQueryExecutorMock;
        string _taxNumber;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _connectionString = "SQLSERVER";

            _birokratQueryExecutorMock = new Mock<IBirokratQueryExecutor>();

            _check = new BirokratDatabase_YearcodeDatabases_Check(
                _birokratQueryExecutorMock.Object,
                _connectionString,
                _taxNumber);
        }

        [Test]
        public async Task Run_AllYearcodeDatabasesArePresent_ReturnsSuccess()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"db-SINHRO", "db-KRATEK", "db-TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_YearcodeDatabasesAreMissing_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"db-SINHRO", "db-KRATEK" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_NoDatabasesForTaxNumber_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Empty);
        }
    }
}
