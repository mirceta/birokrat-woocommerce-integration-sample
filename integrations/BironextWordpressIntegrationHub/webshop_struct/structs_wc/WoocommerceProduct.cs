using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BironextWordpressIntegrationHub.structs
{
    public class WoocommerceProduct
    {

        public string BirokratSifra { get; set; }

        [JsonProperty("additional_data")]
        public Dictionary<string, object> AdditionalData { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("date_created")]
        public DateCreated DateCreated { get; set; }

        [JsonProperty("date_modified")]
        public DateModified DateModified { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("catalog_visibility")]
        public string CatalogVisibility { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("regular_price")]
        public string RegularPrice { get; set; }

        [JsonProperty("sale_price")]
        public string SalePrice { get; set; }

        [JsonProperty("date_on_sale_from")]
        public object DateOnSaleFrom { get; set; }

        [JsonProperty("date_on_sale_to")]
        public object DateOnSaleTo { get; set; }

        [JsonProperty("total_sales")]
        public int TotalSales { get; set; }

        [JsonProperty("tax_status")]
        public string TaxStatus { get; set; }

        [JsonProperty("tax_class")]
        public string TaxClass { get; set; }

        [JsonProperty("manage_stock")]
        public bool ManageStock { get; set; }

        [JsonProperty("stock_quantity")]
        public object StockQuantity { get; set; }

        [JsonProperty("stock_status")]
        public string StockStatus { get; set; }

        [JsonProperty("backorders")]
        public string Backorders { get; set; }

        [JsonProperty("low_stock_amount")]
        public string LowStockAmount { get; set; }

        [JsonProperty("sold_individually")]
        public bool SoldIndividually { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("upsell_ids")]
        public dynamic UpsellIds { get; set; }

        [JsonProperty("cross_sell_ids")]
        public dynamic CrossSellIds { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("reviews_allowed")]
        public bool ReviewsAllowed { get; set; }

        [JsonProperty("purchase_note")]
        public string PurchaseNote { get; set; }

        [JsonProperty("attributes")]
        public dynamic Attributes { get; set; }

        [JsonProperty("default_attributes")]
        public dynamic DefaultAttributes { get; set; }

        [JsonProperty("menu_order")]
        public int MenuOrder { get; set; }

        [JsonProperty("post_password")]
        public string PostPassword { get; set; }

        [JsonProperty("virtual")]
        public bool Virtual { get; set; }

        [JsonProperty("downloadable")]
        public bool Downloadable { get; set; }

        [JsonProperty("category_ids")]
        public dynamic CategoryIds { get; set; }

        [JsonProperty("tag_ids")]
        public dynamic TagIds { get; set; }

        [JsonProperty("shipping_class_id")]
        public int ShippingClassId { get; set; }

        [JsonProperty("downloads")]
        public dynamic Downloads { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("gallery_image_ids")]
        public dynamic GalleryImageIds { get; set; }

        [JsonProperty("download_limit")]
        public int DownloadLimit { get; set; }

        [JsonProperty("download_expiry")]
        public int DownloadExpiry { get; set; }

        [JsonProperty("rating_counts")]
        public dynamic RatingCounts { get; set; }

        [JsonProperty("average_rating")]
        public string AverageRating { get; set; }

        [JsonProperty("review_count")]
        public int ReviewCount { get; set; }

        [JsonProperty("meta_data")]
        public dynamic MetaData { get; set; }

        [JsonProperty("variations")]
        public IList<dynamic> Variations { get; set; }
    }

    public class WoocommerceApiProduct
    {

        public string BirokratSifra { get; set; }

        [JsonProperty("additional_data")]
        public Dictionary<string, object> AdditionalData { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("date_modified")]
        public string DateModified { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("catalog_visibility")]
        public string CatalogVisibility { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("regular_price")]
        public string RegularPrice { get; set; }

        [JsonProperty("sale_price")]
        public string SalePrice { get; set; }

        [JsonProperty("date_on_sale_from")]
        public object DateOnSaleFrom { get; set; }

        [JsonProperty("date_on_sale_to")]
        public object DateOnSaleTo { get; set; }

        [JsonProperty("total_sales")]
        public int TotalSales { get; set; }

        [JsonProperty("tax_status")]
        public string TaxStatus { get; set; }

        [JsonProperty("tax_class")]
        public string TaxClass { get; set; }

        [JsonProperty("manage_stock")]
        public bool ManageStock { get; set; }

        [JsonProperty("stock_quantity")]
        public object StockQuantity { get; set; }

        [JsonProperty("stock_status")]
        public string StockStatus { get; set; }

        [JsonProperty("backorders")]
        public string Backorders { get; set; }

        [JsonProperty("low_stock_amount")]
        public string LowStockAmount { get; set; }

        [JsonProperty("sold_individually")]
        public bool SoldIndividually { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("upsell_ids")]
        public dynamic UpsellIds { get; set; }

        [JsonProperty("cross_sell_ids")]
        public dynamic CrossSellIds { get; set; }

        [JsonProperty("parent_id")]
        public int ParentId { get; set; }

        [JsonProperty("reviews_allowed")]
        public bool ReviewsAllowed { get; set; }

        [JsonProperty("purchase_note")]
        public string PurchaseNote { get; set; }

        [JsonProperty("attributes")]
        public dynamic Attributes { get; set; }

        [JsonProperty("default_attributes")]
        public dynamic DefaultAttributes { get; set; }

        [JsonProperty("menu_order")]
        public int MenuOrder { get; set; }

        [JsonProperty("post_password")]
        public string PostPassword { get; set; }

        [JsonProperty("virtual")]
        public bool Virtual { get; set; }

        [JsonProperty("downloadable")]
        public bool Downloadable { get; set; }

        [JsonProperty("category_ids")]
        public dynamic CategoryIds { get; set; }

        [JsonProperty("tag_ids")]
        public dynamic TagIds { get; set; }

        [JsonProperty("shipping_class_id")]
        public int ShippingClassId { get; set; }

        [JsonProperty("downloads")]
        public dynamic Downloads { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("gallery_image_ids")]
        public dynamic GalleryImageIds { get; set; }

        [JsonProperty("download_limit")]
        public int DownloadLimit { get; set; }

        [JsonProperty("download_expiry")]
        public int DownloadExpiry { get; set; }

        [JsonProperty("rating_counts")]
        public dynamic RatingCounts { get; set; }

        [JsonProperty("average_rating")]
        public string AverageRating { get; set; }

        [JsonProperty("review_count")]
        public int ReviewCount { get; set; }

        [JsonProperty("meta_data")]
        public dynamic MetaData { get; set; }

        [JsonProperty("variations")]
        public IList<dynamic> Variations { get; set; }
    }

    public class Variation
    {

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("availability_html")]
        public string AvailabilityHtml { get; set; }

        [JsonProperty("backorders_allowed")]
        public bool BackordersAllowed { get; set; }

        [JsonProperty("dimensions")]
        public Dimensions Dimensions { get; set; }

        [JsonProperty("dimensions_html")]
        public string DimensionsHtml { get; set; }

        [JsonProperty("display_price")]
        public double DisplayPrice { get; set; }

        [JsonProperty("display_regular_price")]
        public double DisplayRegularPrice { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("image_id")]
        public int ImageId { get; set; }

        [JsonProperty("is_downloadable")]
        public bool IsDownloadable { get; set; }

        [JsonProperty("is_in_stock")]
        public bool IsInStock { get; set; }

        [JsonProperty("is_purchasable")]
        public bool IsPurchasable { get; set; }

        [JsonProperty("is_sold_individually")]
        public string IsSoldIndividually { get; set; }

        [JsonProperty("is_virtual")]
        public bool IsVirtual { get; set; }

        [JsonProperty("max_qty")]
        public string MaxQty { get; set; }

        [JsonProperty("min_qty")]
        public int MinQty { get; set; }

        [JsonProperty("price_html")]
        public string PriceHtml { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("variation_description")]
        public string VariationDescription { get; set; }

        [JsonProperty("variation_id")]
        public int VariationId { get; set; }

        [JsonProperty("variation_is_active")]
        public bool VariationIsActive { get; set; }

        [JsonProperty("variation_is_visible")]
        public bool VariationIsVisible { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("weight_html")]
        public string WeightHtml { get; set; }
    }

}
