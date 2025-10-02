using biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive.common;
using core.logic.common_birokrat;
using NUnit.Framework.Internal;
using si.birokrat.next.common.logging;

namespace biro_to_woo_common_tests
{
    [TestFixture]
    public class ChangeDetectorTests
    {
        private ChangeDetector _changeDetector;
        private string _sifraFieldName;
        private string _skuToBirokrat;
        private bool _verbose;
        private bool _addproducts_notonwebshop;

        [SetUp]
        public void SetUp()
        {
            _sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            _skuToBirokrat = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.Barkoda);
            _verbose = true;
            _addproducts_notonwebshop = false;
            _changeDetector = new ChangeDetector(new ConsoleMyLogger(), _sifraFieldName, _skuToBirokrat, _verbose, _addproducts_notonwebshop);
        }

        // Updated test cases according to new constructor parameters.

        // Happy path
        [Test]
        public void DetectChanges_PriceChanged_ReturnsSifraSet()
        {
            var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {"sku", "1"}, {"regular_price", 100}, {"stock_quantity", 10} }
        };

            var artikli = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {_sifraFieldName, "1"}, {_skuToBirokrat, "1"}, {"PCsPD", "200"}, {"zaloga", "10"} }
        };

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Contains("1"));
        }

        [Test]
        public void DetectChanges_StockChanged_ReturnsSifraSet()
        {
            var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {"sku", "1"}, { "regular_price", 100}, {"stock_quantity", 10} }
        };

            var artikli = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {_sifraFieldName, "1"}, {_skuToBirokrat, "1"}, {"PCsPD", "100"}, { "zaloga", "20"} }
        };

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Contains("1"));
        }

        // Sad path
        [Test]
        public void DetectChanges_NoChanges_ReturnsEmptySet()
        {
            var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {"sku", "1"}, { "regular_price", 100}, {"stock_quantity", 10} }
        };

            var artikli = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {_sifraFieldName, "1"}, {_skuToBirokrat, "1"}, {"PCsPD", "100"}, { "zaloga", "10"} }
        };

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DetectChanges_NoMatchingSku_ReturnsEmptySet()
        {
            var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {"sku", "2"}, {"regular_price", 100}, {"stock_quantity", 10} }
        };

            var artikli = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {_sifraFieldName, "1"}, {_skuToBirokrat, "1"}, {"PCsPD", "100"}, { "zaloga", "10"} }
        };

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Empty);
        }

        // Edge cases
        [Test]
        public void DetectChanges_EmptyProducts_ReturnsEmptySet()
        {
            var products = new List<Dictionary<string, object>>();

            var artikli = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {_sifraFieldName, "1"}, {_skuToBirokrat, "1"}, {"PCsPD", "100"}, { "zaloga", "10"} }
        };

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DetectChanges_EmptyArtikli_ReturnsEmptySet()
        {
            var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { {"sku", "1"}, {"regular_price", 100}, {"stock_quantity", 10} }
        };

            var artikli = new List<Dictionary<string, object>>();

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void DetectChanges_EmptyInputs_ReturnsEmptySet()
        {
            var products = new List<Dictionary<string, object>>();
            var artikli = new List<Dictionary<string, object>>();

            var result = _changeDetector.DetectChanges(products, artikli, new CancellationToken());

            Assert.That(result, Is.Empty);
        }
    }
}
