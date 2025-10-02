using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.logic.mapping_woo_to_biro.document_insertion.postavke_additions {
    public class CommentAddVarAttrs_PostavkaAddOp : IAdditionalOperationOnPostavke {

        bool append;
        public CommentAddVarAttrs_PostavkaAddOp(bool append) {
            this.append = append;
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke) {

            foreach (var postavka in postavke) {
                int idx = postavka.GetOriginalOrderIndex();
                var item = order.Items[idx];

                if (GWooOps.OriginProductAttributeCount(item.OriginProduct) == 0)
                    continue;


                List<KeyValuePair<string, string>> attrs = null;
                try {
                    attrs = GWooOps.ForVariation_WithId_GetAttributeKeyValuePairs(item.OriginProduct, item.VariationId);
                } catch (Exception ex) {
                    attrs = GWooOps.ForVariation_WithSku_GetAttributeKeyValuePairs(item.OriginProduct, item.Sku);
                }

                string comment = string.Join("\n", 
                    attrs.Select(x => FirstLetterToUpper(x.Key) + ": " + FirstLetterToUpper(x.Value)));

                if (append) {
                    if (postavka.Comment == null) 
                        postavka.Comment = "";
                    postavka.Comment += "\n" + comment;
                } else {
                    postavka.Comment = comment;
                }
            }
            return postavke;
        }

        private string FirstLetterToUpper(string str) {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }

    public class DancerkaFixComplexComment : IAdditionalOperationOnPostavke {
        public DancerkaFixComplexComment() { 
        
        }

        public async Task<List<BirokratPostavka>> ApplyOperationToPostavke(WoocommerceOrder order, List<BirokratPostavka> postavke)
        {
            foreach (var postavka in postavke)
            {
                int idx = postavka.GetOriginalOrderIndex();
                var item = order.Items[idx];

                if (GWooOps.OriginProductAttributeCount(item.OriginProduct) == 0)
                    continue;

                List<KeyValuePair<string, string>> attrs = null;
                try
                {
                    attrs = GWooOps.ForVariation_WithId_GetAttributeKeyValuePairs(item.OriginProduct, item.VariationId);
                }
                catch (Exception ex)
                {
                    attrs = GWooOps.ForVariation_WithSku_GetAttributeKeyValuePairs(item.OriginProduct, item.Sku);
                }

                
                string[] rows = postavka.Comment.Split("\n");
                string firstRow = rows[0];
                if (!firstRow.StartsWith("SKU")) continue;
                string attr1 = firstRow.Split(":")[0].Trim();
                string val1 = firstRow.Split(":")[1].Trim();

                string[] parts = val1.Split("/");

                if (parts.Any(x => x.Length < 3)) continue;
                if (parts.Length != rows.Length - 1) continue;

                string[] attrsPars = rows.Skip(1).ToArray();
                var newparts = parts.Zip(attrsPars, (sku, atrow) => new Tuple<string, string>(sku, atrow))
                     .Select((x) => {
                         string sku = x.Item1;
                         string atrow = x.Item2;

                         string preface = atrow.Split(":")[1].Trim();

                         string cmt = "SKU: " + $"{sku}/{preface}";
                         cmt += $"\n{atrow}";

                         return cmt;
                     }).ToList();
                postavka.Comment = string.Join("\n", newparts);
            }



            return postavke;
        }
    }
}
