using gui_attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.structs_wc_to_biro
{
    public class BirokratPostavka
    {
        private int originalOrderSpecificationId;

        public void SaveIndex_WooOrder(int idx) {
            originalOrderSpecificationId = idx;
        }
        public int GetOriginalOrderIndex() {
            return originalOrderSpecificationId;
        }

        public string BirokratSifra { get; set; }
        public int Quantity { get; set; }
        public string Subtotal { get; set; }
        public int DiscountPercent { get; set; }
        public string Comment { get; set; }
        public string StorageIdentifier { get; set; }

    }

    public class TestEqualAddition
    {
        public string biroField;
        public string outField;
        public OutType outType;
        public OutFieldType outFieldType;
        public ArticleType articleType;

        public TestEqualAddition() { }

        [GuiConstructor]
        public TestEqualAddition(string biroField, string outField, OutType outType, OutFieldType outFieldType, ArticleType articleType) { 
            this.biroField = biroField;
            this.outField = outField;
            this.outType = outType;
            this.outFieldType = outFieldType;
            this.articleType = articleType;
        }
    }

    public enum OutType
    {
        WOOCOMMERCE,
        SHOPIFY
    }
    public enum OutFieldType
    {
        STRING,
        VARIABLE_ATTRIBUTE,
        CATEGORY
    }
    public enum ArticleType
    {
        ELEMENTARY,
        VARIABLE,
        BOTH
    }
}
