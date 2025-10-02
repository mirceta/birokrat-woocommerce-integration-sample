using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{
    [TestFixture]
    public class MoreThanOneProductOrVariationContainsSkuTests
    {
        private string skuField;
        private BiroOutComparisonContext context;
        private MoreThanOneProductOrVariationContainsSku validator;

        [SetUp]
        public void Setup()
        {
            skuField = "sku";
            context = new BiroOutComparisonContext();
            validator = new MoreThanOneProductOrVariationContainsSku(skuField);
        }

        [Test]
        public void Verify_WhenNoMatches_ThrowsException()
        {
            string sku = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
            {

            };
            context.outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "sku", "sku2" } }
            };

            Assert.Throws<CannotValidateNonSyncedProductException>(() => validator.Verify(sku, context));
        }

        [Test]
        public void Verify_WhenOneMatch_NoExceptionThrown()
        {
            string sku = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
            {

            };
            context.outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "sku", sku }, { "parent_id", "1" }, { "id", "1" } }
            };

            Assert.DoesNotThrow(() => validator.Verify(sku, context));
        }

        [Test]
        public void Verify_WhenMoreThanOneMatch_ThrowsException()
        {
            string sku = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
            {

            };
            context.outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "sku", sku }, { "parent_id", "1" }, { "id", "1" } },
                new Dictionary<string, object> { { "sku", sku }, { "parent_id", "2" }, { "id", "2" } }
            };

            Assert.Throws<IntegrationProcessingException>(() => validator.Verify(sku, context));
        }

        [Test]
        public void Verify_WhenSkuIsNull_ThrowsNullArgumentException()
        {
            string sku = null;
            context.biroItems = new List<Dictionary<string, object>>
            {

            };
            context.outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "sku", sku }, { "parent_id", "1" }, { "id", "1" } }
            };

            Assert.Throws<ArgumentNullException>(() => validator.Verify(sku, context));

            sku = "";
            Assert.Throws<ArgumentNullException>(() => validator.Verify(sku, context));
        }

        [Test]
        public void Verify_WhenOutItemsIsNull_ThrowsArgNullException()
        {
            string sku = "sku1";
            context.biroItems = new List<Dictionary<string, object>>
            {

            };
            context.outItems = null;

            Assert.Throws<ArgumentNullException>(() => validator.Verify(sku, context));
        }
    }
}
