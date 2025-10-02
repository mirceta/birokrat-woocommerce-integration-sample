using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair_Test
    {
        BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair _check;
        Mock<ISqlUtils> _sqlUtilsMock;
        Mock<IBirokratQueryExecutor> _birokratQueryExecutorMock;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _connectionString;
        bool _doRepair;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _connectionString = "SQLSERVER";
            _doRepair = false;

            _sqlUtilsMock = new Mock<ISqlUtils>();
            _birokratQueryExecutorMock = new Mock<IBirokratQueryExecutor>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new BiroNext_PingerSifraOperaterjaAndPoslovnoLeto_CheckAndRepair(
                _birokratQueryExecutorMock.Object,
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber);
        }

        [Test]
        public async Task Run_PingerApiKeyIsValid_ReturnsSuccess()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetYearcodes_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { "TT#I", "TT#G" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"biro{TextConstants.DELIMITER}TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_NoApiKeyForPinger_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetYearcodes_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { "TT#I", "TT#G" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_NoDatabaseWithPingerYearcode_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetYearcodes_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { "TT#H", "TT#G" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"biro{TextConstants.DELIMITER}TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_NoYearcodeRetrieved_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetYearcodes_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"biro{TextConstants.DELIMITER}TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
