using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.diagnostics.Checks.Database.Utils;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class SqlServer_Collation_Check_Test
    {
        SqlServer_Collation_Check _check;
        Mock<ISqlUtils> _sqlUtilsMock;
        Mock<IBirokratQueryExecutor> _birokratQueryExecutorMock;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _collation;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _collation = "Slovenian";
            _connectionString = "SQLSERVER";

            _sqlUtilsMock = new Mock<ISqlUtils>();
            _birokratQueryExecutorMock = new Mock<IBirokratQueryExecutor>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new SqlServer_Collation_Check(
                _birokratQueryExecutorMock.Object,
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber,
                _collation);
        }

        [Test]
        public async Task Run_CollationCheckIsValid_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.SetupSequence(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"{_collation}" })
                .ReturnsAsync(new List<string> { $"{_collation}" });

            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-AA" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_MainServerCollationIsNull_ReturnsFailure()
        {
            _databaseQueryExecutorMock.SetupSequence(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"NULL" })
                .ReturnsAsync(new List<string> { $"{_collation}" });

            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-AA" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_MainServerWrongCollation_ReturnsFailure()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "Svedish" });

            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-AA" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_ClientDatabaseWrongCollation_ReturnsFailure()
        {
            _databaseQueryExecutorMock.SetupSequence(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { $"{_collation}" })
                .ReturnsAsync(new List<string> { "Svedish" });

            _birokratQueryExecutorMock.Setup(x => x.GetAllDatabases_ThatMatchesTaxNumberAsync(_connectionString, _taxNumber))
                .ReturnsAsync(new List<string> { $"biro{_taxNumber}-AA" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
