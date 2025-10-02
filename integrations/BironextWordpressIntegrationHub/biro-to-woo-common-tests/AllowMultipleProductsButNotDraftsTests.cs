using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{


    [TestFixture]
    public class AllowMultipleProductsButNotDraftsTests
    {
        private BiroOutComparisonContext _context;
        private AllowSKUToBeInMultipleProducts_If_NoneOfTheseProductsIsADraft _operation;

        [SetUp]
        public void Setup()
        {
            _context = new BiroOutComparisonContext
            {
                outItems = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "sku", "sku1" },
                    { "status", "published" },
                    { "type", "simple" },
                    { "parent_id", 0 },
                    { "id", 1 }
                },
                new Dictionary<string, object>
                {
                    { "sku", "sku2" },
                    { "status", "draft" },
                    { "type", "simple" },
                    { "parent_id", 0 },
                    { "id", 2 }
                },
                new Dictionary<string, object>
                {
                    { "sku", "sku3" },
                    { "status", "published" },
                    { "type", "simple" },
                    { "parent_id", 0 },
                    { "id", 3 }
                },
                new Dictionary<string, object>
                {
                    { "sku", "sku3" },
                    { "status", "published" },
                    { "type", "variable" },
                    { "parent_id", 0 },
                    { "id", 4 }
                }
            }
            };

            var skuField = "sku";

            _operation = new AllowSKUToBeInMultipleProducts_If_NoneOfTheseProductsIsADraft(skuField);
        }

        [Test]
        public void Verify_SingleProduct_NoException()
        {
            // Happy path
            Assert.DoesNotThrow(() => _operation.Verify("sku1", _context));
        }

        [Test]
        public void Verify_MultipleProductsOneInDraftStatus_ThrowsIntegrationProcessingException()
        {
            // Sad path
            _context.outItems.Add(new Dictionary<string, object>
            {
                { "sku", "sku2" },
                { "status", "published" },
                { "type", "simple" },
                { "parent_id", 0 },
                { "id", 5 }
            });
            var ex = Assert.Throws<IntegrationProcessingException>(() => _operation.Verify("sku2", _context));
        }

        [Test]
        public void Verify_MultipleProductsDifferentTypes_ThrowsIntegrationProcessingException()
        {
            // Sad path
            var ex = Assert.Throws<IntegrationProcessingException>(() => _operation.Verify("sku3", _context));
        }

        [Test]
        public void Verify_NonexistentSku_ThrowsCannotValidateNonSyncedProductException()
        {
            // Edge case
            var ex = Assert.Throws<CannotValidateNonSyncedProductException>(() => _operation.Verify("sku4", _context));
        }

        [Test]
        public void Verify_NullSku_ThrowsArgumentNullException()
        {
            // Edge case
            Assert.Throws<System.ArgumentNullException>(() => _operation.Verify(null, _context));
        }

        [Test]
        public void Verify_EmptySku_ThrowsCannotValidateNonSyncedProductException()
        {
            // Edge case
            var ex = Assert.Throws<CannotValidateNonSyncedProductException>(() => _operation.Verify("", _context));
            Assert.AreEqual("No products or variations found with the specified sku.", ex.Message);
        }

        [Test]
        public void Verify_NullContext_ThrowsArgumentNullException()
        {
            // Edge case
            Assert.Throws<System.ArgumentNullException>(() => _operation.Verify("sku1", null));
        }
    }

}
