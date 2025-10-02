using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{
    [TestFixture]
    public class ProductOutTypeMismatchTests
    {
        private string variableProductBirokratField;
        private string skuField;
        private BiroOutComparisonContext context;
        private ProductOutTypeMismatch validator;

        [SetUp]
        public void Setup()
        {
            variableProductBirokratField = "variationField";
            skuField = "sku";
            context = new BiroOutComparisonContext();
            validator = new ProductOutTypeMismatch(variableProductBirokratField, skuField);
        }

        [Test]
        public void Verify_WhenBiroHasVariationAndWooHasParent_NoExceptionThrown()
        {
            string sifra = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { skuField, sifra }, { variableProductBirokratField, "variation" } }
        };
            context.outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", sifra }, { "parent_id", "parentId1" } }
        };

            Assert.DoesNotThrow(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_WhenBiroHasVariationAndWooHasNoParent_ThrowsException()
        {
            string sifra = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { skuField, sifra }, { variableProductBirokratField, "variation" } }
        };
            context.outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", sifra }, { "parent_id", null } }
        };

            Assert.Throws<IntegrationProcessingException>(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_WhenBiroHasNoVariationAndWooHasParent_ThrowsException()
        {
            string sifra = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { skuField, sifra }, { variableProductBirokratField, null } }
        };
            context.outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", sifra }, { "parent_id", "parentId1" } }
        };

            Assert.Throws<IntegrationProcessingException>(() => validator.Verify(sifra, context));
        }

        [Test]
        public void Verify_MultipleWooMatchesWithMismatch_ThrowsException()
        {
            var biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", "sku1" }, { "variableProductBirokratField", "value1" } }
        };

            var outItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", "sku1" }, { "parent_id", "1" } },
            new Dictionary<string, object> { { "sku", "sku1" }, { "parent_id", "0" } }
        };

            var context = new BiroOutComparisonContext { biroItems = biroItems, outItems = outItems };
            var productOutTypeMismatch = new ProductOutTypeMismatch("variableProductBirokratField", "sku");

            Assert.Throws<IntegrationProcessingException>(() => productOutTypeMismatch.Verify("sku1", context));
        }
    }
}
