using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using Newtonsoft.Json;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{
    [TestFixture]
    public class ProductHasDifferentAttributesThanArticleTests
    {
        private string skuField;
        private Dictionary<string, string> allPossibleAdditionAttrBiroToOut;
        private BiroOutComparisonContext context;
        private ProductHasDifferentAttributesThanArticle validator;

        [SetUp]
        public void Setup()
        {
            skuField = "sku";
            allPossibleAdditionAttrBiroToOut = new Dictionary<string, string> { { "size", "length" }, { "color", "colour" } };
            context = new BiroOutComparisonContext();
            validator = new ProductHasDifferentAttributesThanArticle(skuField, allPossibleAdditionAttrBiroToOut);
        }

        [Test]
        public void Verify_WhenAttributeValuesMatch_NoExceptionThrown()
        {
            string sifra = "sku1";
            var attributes = new[] { new { name = "length", option = "M" }, new { name = "colour", option = "Blue" } };
            context.outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", sifra }, { "attributes", attributes } }
        };
            context.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { skuField, sifra }, { "size", "M" }, { "color", "Blue" } }
        };

            Assert.DoesNotThrow(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_WhenAttributeValuesDoNotMatch_ThrowsException()
        {
            string sifra = "sku1";
            var attributes = new[] { new { name = "length", option = "L" }, new { name = "colour", option = "Blue" } };
            context.outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "sku", sifra }, { "attributes", attributes } }
            };
            context.biroItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { skuField, sifra }, { "size", "M" }, { "color", "Blue" } }
            };

            Assert.Throws<IntegrationProcessingException>(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_WhenOutItemDoesNotExist_ThrowsException()
        {
            string sifra = "sku1";
            context.outItems = new List<Dictionary<string, object>>();
            context.biroItems = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { skuField, sifra }, { "size", "M" }, { "color", "Blue" } }
                };

            Assert.Throws<KeyNotFoundException>(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_WhenBiroItemDoesNotExist_ThrowsException()
        {
            string sifra = "sku1";
            var attributes = new[] { new { name = "length", option = "M" }, new { name = "colour", option = "Blue" } };
            context.outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", sifra }, { "attributes", JsonConvert.SerializeObject(attributes) } }
        };
            context.biroItems = new List<Dictionary<string, object>>();

            Assert.Throws<KeyNotFoundException>(() => validator.Verify(sifra, context));
        }
    }
}
