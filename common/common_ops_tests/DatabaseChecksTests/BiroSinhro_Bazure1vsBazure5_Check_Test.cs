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
    public class BiroSinhro_Bazure1vsBazure5_Check_Test
    {
        BiroSinhro_Bazure1vsBazure5_Check _check;
        Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;

        [SetUp]
        public void SetUp()
        {
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();
            _check = new BiroSinhro_Bazure1vsBazure5_Check(_databaseQueryExecutorMock.Object);
        }

        [Test]
        public async Task Run_ServersAreSynchronized_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string>
                    {
                        "1200||Vault Tek||00000001||2025||8044063||8044000||21#M||0||8044000||21#M||0",
                        "1230||West-Tek||00000002||2025||8044063||8044000||AL3M||0||8044000||AL3M||0",
                        "1234||CIT||00000003||2025||0||0||AS#M||0||0||AS#M||0"
                    } );

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Empty);
        }

        [Test]
        public async Task Run_FilesAreNotSynchronized_ReturnsSuccess()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string>
                    {
                        "1200||Vault Tek||00000001||2025||8044063||8044000||21#M||0||8044000||21#M||0",
                        "1230||West-Tek||00000002||2025||8045063||8044000||AL3M||0||8044000||AL3M||5",
                        "1234||CIT||00000003||2025||0||0||AS#M||0||0||AS#M||0"
                    });

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
            Assert.That(result.AdditionalInfo, Is.Not.Empty);
        }

        [Test]
        public async Task Run_NoDataRecievedFromEndpoint_ReturnsFailure()
        {
            _databaseQueryExecutorMock.Setup(x => x.CreateSqlReader_ThenExecuteAndReturnAllRowsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string>{ });

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
            Assert.That(result.AdditionalInfo.Any(x => x.Contains("Error", System.StringComparison.OrdinalIgnoreCase)), Is.True);
        }
    }
}
