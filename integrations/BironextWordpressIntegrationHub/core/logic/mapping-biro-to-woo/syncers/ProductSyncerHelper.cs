using birowoo_exceptions;
using core.tools.wooops;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.mapping_biro_to_woo.syncers
{
    public class ProductSyncerHelper
    {

        public static void ValidateChanges(Dictionary<string, object> biroArtikel, string body, string res) {
            try {
                GWooOps.ThrowExceptionIfProductPostWooApiCallFailed(body, res);
            } catch (WooCallFailException ex) {
                throw new ProductUpdatingException(ex.Message, ex);
            }
            try {
                BiroProductSyncerHelper.areTheSame(biroArtikel, res);
            } catch (ProductStillDifferentThanArtikelAfterUpdateException ex) {
                ValidateSeparator(biroArtikel, body, res);
                throw new ProductUpdatingException(ex.Message, ex);
            }
        }


        private static void ValidateSeparator(Dictionary<string, object> biroArtikel, string body, string res) {
            try {
                BiroProductSyncerHelper.separatorNotRecognized(biroArtikel, res);
            } catch (ProductStillDifferentThanArtikelAfterUpdateException ex) {

                throw new ProductUpdatingException(ex.Message, ex);
            }
        }

    }
}
