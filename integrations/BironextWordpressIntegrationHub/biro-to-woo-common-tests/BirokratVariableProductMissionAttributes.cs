using biro_to_woo_common.executor.validation_stages.validators.validation_operations;
using birowoo_exceptions;
using core.structs;
using NUnit.Framework.Internal;

namespace biro_to_woo_common_tests
{
    [TestFixture]
    public class BirokratVariableProductMissionAttributes
    {
        private Dictionary<string, string> _allPossibleAdditionAttrBiroToOut;
        private BiroOutComparisonContext _comparisonContext;
        private BirokratVariableArtikelMissingAttributes _birokratVariableArtikelMissingAttributes;

        [SetUp]
        public void Setup()
        {
            _allPossibleAdditionAttrBiroToOut = new Dictionary<string, string> { { "attribute1", "attr1" }, { "attribute2", "attr2" } };
            _comparisonContext = new BiroOutComparisonContext();
            _birokratVariableArtikelMissingAttributes = new BirokratVariableArtikelMissingAttributes("variableProductBirokratField", "sku", _allPossibleAdditionAttrBiroToOut);
        }

        [Test]
        public void Verify_NoMatchingArtikel_ThrowsException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>();
            Assert.Throws<IntegrationProcessingException>(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_MultipleMatchingArtikel_ThrowsException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "sku", "sku1" } },
            new Dictionary<string, object> { { "sku", "sku1" } }
        };
            Assert.Throws<IntegrationProcessingException>(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_VariableProductWithAllAttributes_DoesNotThrowException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "sku", "sku1" },
                { "variableProductBirokratField", "value1" },
                { "attribute1", "attr_value1" },
                { "attribute2", "attr_value2" }
            },
            new Dictionary<string, object>
            {
                { "sku", "sku2" },
                { "variableProductBirokratField", "value1" },
                { "attribute1", "attr_value3" },
                { "attribute2", "attr_value4" }
            }
        };
            Assert.DoesNotThrow(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_VariableProductMissingAttribute_ThrowsException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "sku", "sku1" },
                { "variableProductBirokratField", "value1" },
                { "attribute1", "attr_value1" },
                { "attribute2", "attr_value2" }
            },
            new Dictionary<string, object>
            {
                { "sku", "sku2" },
                { "variableProductBirokratField", "value1" },
                { "attribute1", null },
                { "attribute2", "attr_value2" }
            }
        };
            Assert.Throws<IntegrationProcessingException>(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_SingleProduct_DoesNotThrowException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "sku", "sku1" },
                { "variableProductBirokratField", null },
                { "attribute1", "attr_value1" },
                { "attribute2", "attr_value2" }
            }
        };
            Assert.DoesNotThrow(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_NoProduct_ThrowsException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>();
            Assert.Throws<IntegrationProcessingException>(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }

        [Test]
        public void Verify_ProductWithNoAttributes_ThrowsException()
        {
            _comparisonContext.biroItems = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "sku", "sku1" },
                { "variableProductBirokratField", "value1" },
                { "attribute1", null },
                { "attribute2", null }
            }
        };
            Assert.Throws<IntegrationProcessingException>(() => _birokratVariableArtikelMissingAttributes.Verify("sku1", _comparisonContext));
        }
    }
}
