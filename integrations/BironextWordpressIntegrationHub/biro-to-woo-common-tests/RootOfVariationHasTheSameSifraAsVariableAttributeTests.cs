using birowoo_exceptions;
using core.structs;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{

    using NUnit.Framework;
    using Moq;
    using System.Collections.Generic;
    using biro_to_woo_common.executor.validation.validation_stages.validators.validation_operations;

    [TestFixture]
    public class RootOfVariationHasTheSameSifraAsVariableAttributeTests
    {
        private RootOfVariationHasTheSameSifraAsVariableAttribute _attribute;
        private BiroOutComparisonContext _context;

        [SetUp]
        public void SetUp()
        {
            _attribute = new RootOfVariationHasTheSameSifraAsVariableAttribute("sku", "variation");
        }

        [Test]
        public void Verify_NullSifra_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            Assert.Throws<ArgumentNullException>(() => _attribute.Verify(null, _context));
        }

        [Test]
        public void Verify_NullContext_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _attribute.Verify("sifra1", null));
        }

        [Test]
        public void Verify_NoBiroItemFound_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            Assert.Throws<IntegrationProcessingException>(() => _attribute.Verify("nonexistent", _context));
        }

        [Test]
        public void Verify_NoMatchesFound_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            _context.biroItems.Add(new Dictionary<string, object> { { "sku", "sifra1" } });
            Assert.Throws<IntegrationProcessingException>(() => _attribute.Verify("sifra1", _context));
        }

        [Test]
        public void Verify_NoParentFound_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            _context.biroItems.Add(new Dictionary<string, object> { { "sku", "sifra1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "2"}, { "sku", "sifra1" }, { "parent_id", "nonexistent" } });
            Assert.Throws<IntegrationProcessingException>(() => _attribute.Verify("sifra1", _context));
        }

        [Test]
        public void Verify_MultipleParentsFound_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            _context.biroItems.Add(new Dictionary<string, object> { { "sku", "sifra1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "2" }, { "sku", "sifra1" }, { "parent_id", "parent1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "parent1" }, { "sku", "variation1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "parent1" }, { "sku", "variation2" } });
            Assert.Throws<IntegrationProcessingException>(() => _attribute.Verify("sifra1", _context));
        }

        [Test]
        public void Verify_ParentSkuDoesNotMatchBiroItemVariation_ThrowsException()
        {
            _context = new BiroOutComparisonContext();
            _context.biroItems.Add(new Dictionary<string, object> { { "sku", "sifra1" }, { "variation", "variation1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "2" }, { "sku", "sifra1" }, { "parent_id", "parent1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "parent1" }, { "sku", "nonmatching" } });
            Assert.Throws<IntegrationProcessingException>(() => _attribute.Verify("sifra1", _context));
        }

        [Test]
        public void Verify_ValidData_DoesNotThrowException()
        {
            _context = new BiroOutComparisonContext();
            _context.biroItems.Add(new Dictionary<string, object> { { "id", "1" }, { "sku", "sifra1" }, { "variation", "variation1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "2" }, { "sku", "sifra1" }, { "parent_id", "parent1" } });
            _context.outItems.Add(new Dictionary<string, object> { { "id", "parent1" }, { "sku", "variation1" } });
            Assert.DoesNotThrow(() => _attribute.Verify("sifra1", _context));
        }
    }

}