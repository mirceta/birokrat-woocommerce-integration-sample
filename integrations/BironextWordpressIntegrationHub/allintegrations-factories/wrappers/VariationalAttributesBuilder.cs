using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiroWoocommerceHub.structs_wc_to_biro;
using BiroWoocommerceHubTests.tools;

namespace allintegrations_factories.wrappers
{
    class VariationalAttributesBuilder
    {

        List<TestEqualAddition> testEqualAdditions;
        List<Tuple<string, WooAttr>> additionalAttrs;
        public VariationalAttributesBuilder()
        {
            testEqualAdditions = new List<TestEqualAddition>();
            additionalAttrs = new List<Tuple<string, WooAttr>>();
        }

        public VariationalAttributesBuilder AddVariationAttribute(string birokratCode, WooAttr wooAttr)
        {
            additionalAttrs.Add(new Tuple<string, WooAttr>(birokratCode, wooAttr));
            testEqualAdditions.Add(new TestEqualAddition
            {
                biroField = birokratCode,
                outField = wooAttr.Name,
                outType = OutType.WOOCOMMERCE,
                articleType = ArticleType.BOTH,
                outFieldType = OutFieldType.VARIABLE_ATTRIBUTE
            });
            return this;
        }

        public async Task<BirokratArtikelToWooProductMapping> AppendAttributes(BirokratArtikelToWooProductMapping mapping)
        {
            foreach (var x in additionalAttrs)
            {
                await mapping.AddAttributeMapping(x.Item1, x.Item2);
            }
            return mapping;
        }

        public List<TestEqualAddition> GetTestEqualAdditions() { return testEqualAdditions; }
    }
}
