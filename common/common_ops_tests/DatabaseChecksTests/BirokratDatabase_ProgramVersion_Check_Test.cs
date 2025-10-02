using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class BirokratDatabase_ProgramVersion_Check_Test
    {
        BirokratDatabase_ProgramVersion_Check _check;
        Mock<ISqlUtils> _sqlUtilsMock;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        string _taxNumber;
        string _connectionString;
        string _birokratVersion;

        [SetUp]
        public void SetUp()
        {
            _taxNumber = "00000000";
            _birokratVersion = "10.10.10";
            _connectionString = "SQLSERVER";

            _sqlUtilsMock = new Mock<ISqlUtils>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();

            _check = new BirokratDatabase_ProgramVersion_Check(
                _databaseQueryExecutorMock.Object,
                _connectionString,
                _taxNumber,
                _birokratVersion);
        }

        [Test]
        public async Task Run_ProgramVersionsMatch_ReturnsTrue()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "10010010" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith("ERRROR", System.StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith("WARNING", System.StringComparison.OrdinalIgnoreCase)), Is.False);
        }

        [Test]
        public async Task Run_ProgramMinorVersionDoNotMatch_ReturnsTrue()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "10010000" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith("ERROR", System.StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith("WARNING", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task Run_ProgramMajorVersionDoNotMatch_ReturnsFailure()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(_connectionString, It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "10020010" });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.EndsWith("ERROR", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }

    }
}
