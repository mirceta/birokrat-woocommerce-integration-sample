using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class BiroNext_ApiKeys_Check_Test
    {
        BiroNext_ApiKeys_Check _check;
        Mock<ISqlUtils> _sqlUtilsMock;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _connectionString = "SQLSERVER";

            _sqlUtilsMock = new Mock<ISqlUtils>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new BiroNext_ApiKeys_Check(
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber);
        }

        [Test]
        public async Task Run_ApiKeyFound_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"somekey{TextConstants.DELIMITER}Someone{TextConstants.DELIMITER}BETA" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_MultipleApiKeysFound_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string>
                {
                    $"somekey{TextConstants.DELIMITER}Someone1{TextConstants.DELIMITER}BETA",
                    $"somekey{TextConstants.DELIMITER}Someone2{TextConstants.DELIMITER}PROD",
                    $"somekey{TextConstants.DELIMITER}Someone3{TextConstants.DELIMITER}BETA"
                });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_ApiKeysNotFound_ReturnsFailure()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo, Is.Empty);
        }
    }
}
