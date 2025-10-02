using birowoo_exceptions;
using core.structs;
using System.Collections.Generic;
using System.Linq;

namespace biro_to_woo_common.executor.validation_stages.validators.validation_operations
{
    // Responsibility: The purpose of the class is to find all items within biroItems,
    // that have the same value for variableProductBirokratField - call this set S.
    // You want to verify that any element in set S has non null value for all attributes
    // for which it holds true that there exist an element in set S for which the value of
    // the attribute is not null or empty.
    // By example: All variations of Baseballcap, must have a color. If there exists a baseballcap
    //             with a size, then all Baseballcaps must have a size.
    public class BirokratVariableArtikelMissingAttributes : IProductTransferVerifyOperation
    {

        string variableProductBirokratField;
        string skuField;
        Dictionary<string, string> allPossibleAdditionAttrBiroToOut;

        public BirokratVariableArtikelMissingAttributes(string variableProductBirokratField,
            string skuField,
            Dictionary<string, string> allPossibleAdditionAttrBiroToOut)
        {
            this.variableProductBirokratField = variableProductBirokratField;
            this.skuField = skuField;
            this.allPossibleAdditionAttrBiroToOut = allPossibleAdditionAttrBiroToOut;
        }

        public void Verify(string sifra, BiroOutComparisonContext comparisonContext)
        {

            var matches = comparisonContext.biroItems.Where(x => x[skuField] as string == sifra);

            if (matches.Count() == 0)
                throw new IntegrationProcessingException($"There is no artikel in Birokrat with sifra {sifra}");
            if (matches.Count() > 1)
                throw new IntegrationProcessingException($"There are are multiple articles in birokrat with the same sifra {sifra}");

            var match = matches.Single();

            if (string.IsNullOrEmpty(match[variableProductBirokratField] as string))
            {
                return; // this is not a variable product
            }

            if (allPossibleAdditionAttrBiroToOut.All(x => string.IsNullOrEmpty(match[x.Key] as string)))
            {
                string msg = $"V Birokratu je artikel {match[skuField]} variacijski (ima nastavljeno polje {variableProductBirokratField}),";
                msg += "vendar nima nastavljenega nobenega variacijskega atributa v Birokratu.";
               throw new IntegrationProcessingException(msg);
            }

            IEnumerable<Dictionary<string, object>> biroAllVariationsOfOneProduct = FindAllVariationsOfProduct(comparisonContext, match);
            Dictionary<string, string> izpolnjeniAtributiVSifro = FindAllNonNullAdditionalAttributes(biroAllVariationsOfOneProduct);
            List<string> problems = VerifyAllVariationsHaveTheSameAdditionalAttrs(biroAllVariationsOfOneProduct, izpolnjeniAtributiVSifro);
            if (problems != null && problems.Count != 0)
                throw new IntegrationProcessingException(string.Join(";", problems));

        }

        #region [auxiliary]
        private List<string> VerifyAllVariationsHaveTheSameAdditionalAttrs(IEnumerable<Dictionary<string, object>> biroAllVariationsOfOneProduct, Dictionary<string, string> izpolnjeniAtributiVSifro)
        {
            List<string> problems = new List<string>();
            foreach (var biroVariation in biroAllVariationsOfOneProduct)
            {
                foreach (var izpolnjenAttr in izpolnjeniAtributiVSifro.Keys)
                {
                    if (string.IsNullOrEmpty(biroVariation[izpolnjenAttr] as string))
                    {
                        string currerr = "";
                        currerr += "Vsi artikli z isto vrednostjo variacijskega polja morajo imeti nastavljene iste variacijske atribute";
                        currerr += "na različne vrednosti. Naprimer, če imamo hlače treh različnih barv, potem ne moremo narediti četrtih";
                        currerr += "hlač, kjer barva ni nastavljena.  .";
                        currerr += $"Sku {biroVariation[skuField]} ima atribut {allPossibleAdditionAttrBiroToOut[izpolnjenAttr]} (biro: {izpolnjenAttr}) prazen, a artikel z istim varicijskim atributom";
                        currerr += $"(Sku: {izpolnjeniAtributiVSifro[izpolnjenAttr as string]}) ima ta atribut izpolnjen.";
                        problems.Add(currerr);
                    }
                }
            }
            return problems;
        }

        private IEnumerable<Dictionary<string, object>> FindAllVariationsOfProduct(BiroOutComparisonContext comparisonContext, Dictionary<string, object> match)
        {
            return comparisonContext.biroItems.Where(x => x[variableProductBirokratField] as string == match[variableProductBirokratField] as string);
        }

        private Dictionary<string, string> FindAllNonNullAdditionalAttributes(IEnumerable<Dictionary<string, object>> biroAllVariationsOfOneProduct)
        {
            Dictionary<string, string> izpolnjeniAtributiVSifro = new Dictionary<string, string>();
            foreach (var biroItem in biroAllVariationsOfOneProduct)
            {
                foreach (var possibleAttr in allPossibleAdditionAttrBiroToOut.Keys)
                {
                    if (!string.IsNullOrEmpty(biroItem[possibleAttr] as string))
                    {
                        izpolnjeniAtributiVSifro[possibleAttr] = biroItem[skuField] as string;
                    }
                }
            }

            return izpolnjeniAtributiVSifro;
        }
        #endregion
    }
}
