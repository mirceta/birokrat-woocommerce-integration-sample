using common_ops.diagnostics.Checks.Database.Checks;
using common_ops.Executors.Sql;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace common_ops_tests.DatabaseChecksTests
{
    [TestFixture]
    public class SqlServer_Connection_Check_Test
    {
        SqlServer_Connection_Check _check;
        Mock<ISqlUtils> _sqlUtilsMock;
        string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = "SQLSERVER";
            _sqlUtilsMock = new Mock<ISqlUtils>();
            _check = new SqlServer_Connection_Check(_sqlUtilsMock.Object, _connectionString);
        }

        [Test]
        public async Task Run_VerifySqlServer_ReturnSuccessAsync()
        {
            _sqlUtilsMock.Setup(x => x.CheckSqlServer(It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _check.Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_VerifySqlServer_ReturnFailureAsync()
        {
            _sqlUtilsMock.Setup(x => x.CheckSqlServer(It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _check.Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
