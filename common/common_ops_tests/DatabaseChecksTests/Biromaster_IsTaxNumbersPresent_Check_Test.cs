using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class Biromaster_IsTaxNumbersPresent_Check_Test
    {
        Biromaster_IsTaxNumbersPresent_Check _check;

        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _connectionString = "SQLSERVER";

            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new Biromaster_IsTaxNumbersPresent_Check(
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber);
        }

        [Test]
        public async Task Run_TaxNumberIsPresentInBiromaster_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "company name" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_TaxNumberIsNotPresentInBiromaster_ReturnsFailure()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Empty);
        }
    }
}
