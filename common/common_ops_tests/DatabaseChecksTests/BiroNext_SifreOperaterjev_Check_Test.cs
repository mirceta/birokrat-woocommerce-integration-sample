using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.diagnostics.Constants;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class BiroNext_SifreOperaterjev_Check_Test
    {
        BiroNext_SifreOperaterjev_Check _check;
        Mock<IBirokratQueryExecutor> _birokratQueryExecutorMock;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _connectionString;
        string _collation;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _connectionString = "SQLSERVER";

            _birokratQueryExecutorMock = new Mock<IBirokratQueryExecutor>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new BiroNext_SifreOperaterjev_Check(
                _birokratQueryExecutorMock.Object,
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber);
        }

        [Test]
        public async Task Run_AllOperaterYearcodesAreValid_ReturnsSuccess()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-TT#I", $"biro{_taxNumber}-TT#L", $"biro{_taxNumber}-TT#M" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"First{TextConstants.DELIMITER}TT#I", $"Second{TextConstants.DELIMITER}TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_AtLeastOneOperaterYearcodeIsValid_ReturnsSuccess()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-TT#I", $"biro{_taxNumber}-TT#L", $"biro{_taxNumber}-TT#M" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"First{TextConstants.DELIMITER}TT#G", $"Second{TextConstants.DELIMITER}TT#I" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.All(x => x.Contains("ERROR", System.StringComparison.OrdinalIgnoreCase)), Is.False);
        }

        [Test]
        public async Task Run_NoOperaterYearcodeIsValid_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-TT#I", $"biro{_taxNumber}-TT#L", $"biro{_taxNumber}-TT#M" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"First{TextConstants.DELIMITER}TT#G", $"Second{TextConstants.DELIMITER}TT#G" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.All(x => x.Contains("ERROR", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_NoOperaterRetrieved_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-TT#I", $"biro{_taxNumber}-TT#L", $"biro{_taxNumber}-TT#M" });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_NoYearcodeDatabaseRetrieved_ReturnsFailure()
        {
            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { });

            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"First{TextConstants.DELIMITER}TT#G", $"Second{TextConstants.DELIMITER}TT#G" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.All(x => x.Contains("ERROR", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }
    }
}
