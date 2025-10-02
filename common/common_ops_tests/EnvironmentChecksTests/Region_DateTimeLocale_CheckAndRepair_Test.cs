using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace common_ops_tests.EnvironmentChecksTests
{
    [TestFixture]
    public class Region_DateTimeLocale_CheckAndRepair_Test
    {
        Mock<ICultureInfoHelper> _cultureInfoHelperMock;
        Mock<IRegistryUtils> _registryUtilsMock;

        public Region_DateTimeLocale_CheckAndRepair BuildCheck(bool doRepair)
        {
            return new Region_DateTimeLocale_CheckAndRepair(
                _cultureInfoHelperMock.Object,
                _registryUtilsMock.Object,
                doRepair,
                (message) => doRepair);
        }

        [SetUp]
        public void Setup()
        {
            _cultureInfoHelperMock = new Mock<ICultureInfoHelper>();
            _registryUtilsMock = new Mock<IRegistryUtils>();
        }

        [Test]
        public async Task Run_DateTimeAndCultureIsCorrect_ReturnsSuccess()
        {
            CultureInfo culture = new CultureInfo("sl-SI"); 
            var dateTimeFormat = culture.DateTimeFormat;
            dateTimeFormat.ShortDatePattern = "dd.MM.yyyy";

            _cultureInfoHelperMock.Setup(x => x.GetCurrentCulture())
                .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.GetDateTimeFormat())
                .Returns(dateTimeFormat);

            _cultureInfoHelperMock.Setup(x => x.BuildCultureInfoFromString(It.IsAny<string>()))
               .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.ParseCulture(It.IsAny<string>()))
                .Returns(culture.DisplayName);

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
               .Returns("sl-SI")
               .Returns("0424") //this is Slovenia locale code
               .Returns("0424");

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Callback(() => { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_LocaleRegionIsWrong_ReturnsFailure()
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            var dateTimeFormat = culture.DateTimeFormat;

            _cultureInfoHelperMock.Setup(x => x.GetCurrentCulture())
                .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.GetDateTimeFormat())
                .Returns(dateTimeFormat);

            _cultureInfoHelperMock.Setup(x => x.BuildCultureInfoFromString(It.IsAny<string>()))
               .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.ParseCulture(It.IsAny<string>()))
                .Returns(culture.DisplayName);

            _registryUtilsMock.SetupSequence(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
               .Returns("en-US")
               .Returns("1033") 
               .Returns("0424");

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Callback(() => { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_WrongDateAndTimeFormatButRepairIsTrue_ReturnsSuccess()
        {
            CultureInfo culture = new CultureInfo("en-US");
            var dateTimeFormat = culture.DateTimeFormat;

            _cultureInfoHelperMock.Setup(x => x.GetCurrentCulture())
                .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.GetDateTimeFormat())
                .Returns(dateTimeFormat);

            _cultureInfoHelperMock.Setup(x => x.BuildCultureInfoFromString(It.IsAny<string>()))
               .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.ParseCulture(It.IsAny<string>()))
                .Returns(culture.DisplayName);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0424"); //this is Slovenia locale code

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Callback(() => { });

            var result = await BuildCheck(true).Run();

            Assert.That(result.Result, Is.True);
        }

        [Test]
        public async Task Run_WrongDateAndTimeFormat_ReturnsFailure()
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            var dateTimeFormat = culture.DateTimeFormat;

            _cultureInfoHelperMock.Setup(x => x.GetCurrentCulture())
                .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.GetDateTimeFormat())
                .Returns(new CultureInfo("en-US").DateTimeFormat);

            _cultureInfoHelperMock.Setup(x => x.BuildCultureInfoFromString(It.IsAny<string>()))
               .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.ParseCulture(It.IsAny<string>()))
                .Returns(culture.DisplayName);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0000"); 

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Callback(() => { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task Run_WrongCultureButCorrectDateTime_ReturnsFailure()
        {
            CultureInfo culture = new CultureInfo("sl-SI");
            var dateTimeFormat = culture.DateTimeFormat;

            _cultureInfoHelperMock.Setup(x => x.GetCurrentCulture())
                .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.GetDateTimeFormat())
                .Returns(dateTimeFormat);

            _cultureInfoHelperMock.Setup(x => x.BuildCultureInfoFromString(It.IsAny<string>()))
               .Returns(culture);

            _cultureInfoHelperMock.Setup(x => x.ParseCulture(It.IsAny<string>()))
                .Returns(culture.DisplayName);

            _registryUtilsMock.Setup(x => x.GetRegistryValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("0000");

            _registryUtilsMock.Setup(x => x.FixRegistryValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .Callback(() => { });

            var result = await BuildCheck(false).Run();

            Assert.That(result.Result, Is.False);
        }
    }
}
