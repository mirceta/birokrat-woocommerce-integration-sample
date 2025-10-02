using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace BiroWoocommerceHubTests
{

    public class OutApiClientDecorator : IOutApiClient
    {
        private readonly IOutApiClient _next;

        public OutApiClientDecorator(IOutApiClient next)
        {
            _next = next;
        }

        public string Ck => _next.Ck;

        public string Cs => _next.Cs;

        public string Address => _next.Address;

        public virtual void SetLogger(IMyLogger logger) => _next.SetLogger(logger);


        public virtual async Task<string> Get(string msg)
        {
            return await _next.Get(msg);
        }

        public virtual async Task<List<Dictionary<string, object>>> GetProducts()
        {
            return await _next.GetProducts();
        }

        public virtual async Task<ProductResult> GetProductBySku(string sku)
        {
            return await _next.GetProductBySku(sku);
        }

        public virtual async Task<Dictionary<string, object>> UpdateProduct(string id, Dictionary<string, object> values)
        {
            return await _next.UpdateProduct(id, values);
        }

        public virtual async Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product)
        {
            return await _next.PostProduct(product);
        }

        public virtual async Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product)
        {
            return await _next.PostBaseVariableProduct(product);
        }

        public virtual async Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation)
        {
            return await _next.PostVariation(parent_id, variation);
        }

        public virtual async Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values)
        {
            return await _next.UpdateVariation(product_id, variation_id, values);
        }

        public virtual async Task<List<Dictionary<string, object>>> GetVariableProducts()
        {
            return await _next.GetVariableProducts();
        }


        public virtual async Task<string> DeleteProductBySku(string sku)
        {
            return await _next.DeleteProductBySku(sku);
        }

        public virtual async Task<string> DeleteProduct(string id)
        {
            return await _next.DeleteProduct(id);
        }

        public virtual async Task<string> DeleteVariation(string parent_id, string variation_id)
        {
            return await _next.DeleteVariation(parent_id, variation_id);
        }

        public virtual async Task<List<Dictionary<string, object>>> GetAttributes()
        {
            return await _next.GetAttributes();
        }

        public virtual async Task<List<Dictionary<string, object>>> GetAttributes(string productId)
        {
            return await _next.GetAttributes(productId);
        }

        public virtual async Task<Dictionary<string, object>> PostAttribute(Dictionary<string, object> attribute)
        {
            return await _next.PostAttribute(attribute);
        }

        public virtual async Task<List<Dictionary<string, object>>> GetAttributeTerms(string attributeId)
        {
            return await _next.GetAttributeTerms(attributeId);
        }

        public virtual async Task<Dictionary<string, object>> PostAttributeTerm(string attributeId, string attributeTerm)
        {
            return await _next.PostAttributeTerm(attributeId, attributeTerm);
        }

        public virtual async Task<List<Category>> GetCategories()
        {
            return await _next.GetCategories();
        }

        public virtual async Task<Category> PostCategory(string name)
        {
            return await _next.PostCategory(name);
        }

        public virtual async Task<string> GetKita(string query)
        {
            return await _next.GetKita(query);
        }

        public virtual async Task<string> PutKita(string query, string body)
        {
            return await _next.PutKita(query, body);
        }

        public virtual async Task<string> PostKita(string query, string body)
        {
            return await _next.PostKita(query, body);
        }
        public virtual async Task<string> DeleteKita(string query)
        {
            return await _next.DeleteKita(query);
        }

        public virtual async Task<string> MyGetOrder(string id)
        {
            return await _next.MyGetOrder(id);
        }

        public virtual async Task<List<string>> GetOrders(DateTime sinceDate)
        {
            return await _next.GetOrders(sinceDate);
        }

        public virtual async Task<List<string>> GetOrderDescriptions(DateTime sinceDate)
        {
            return await _next.GetOrderDescriptions(sinceDate);
        }

        public virtual async Task<List<List<StatusChange>>> GetOrderStatusChanges(List<int> orderIds)
        {
            return await _next.GetOrderStatusChanges(orderIds);
        }

        
    }
    public class VoidOutApiClient : IOutApiClient
    {
        public string Ck => throw new System.NotImplementedException();

        public string Cs => throw new System.NotImplementedException();

        public string Address => throw new System.NotImplementedException();

        public Task<string> AddUnacceptedOrderTransfer(string orderId, string orderStatus)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> DeleteKita(string query)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> DeleteOrderTransfer(string orderid, string orderstatus)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> DeleteProduct(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> DeleteProductBySku(string sku)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteProductTransfer(string productid)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> DeleteVariation(string parent_id, string variation_id)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> Get(string msg)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetAttributes()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetAttributes(string productId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetAttributeTerms(string attributeId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Category>> GetCategories()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetKita(string query)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<string>> GetOrderDescriptions(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetOrders(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<StatusChange>> GetOrderStatusChanges(List<int> orderIds)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetOrderTransfer(string orderTransfer)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetOrderTransfers()
        {
            throw new System.NotImplementedException();
        }

        public Task<ProductResult> GetProductBySku(string sku)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetProducts()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Dictionary<string, object>>> GetVariableProducts()
        {
            throw new System.NotImplementedException();
        }


        public Task<string> MyGetOrder(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> PostAttribute(Dictionary<string, object> attribute)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> PostAttributeTerm(string attributeId, string attributeTerm)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product)
        {
            throw new System.NotImplementedException();
        }

        public Task<Category> PostCategory(string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> PostKita(string query, string body)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> PutKita(string query, string body)
        {
            throw new System.NotImplementedException();
        }

        public void SetLogger(IMyLogger logger)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> UpdateProduct(string id, Dictionary<string, object> values)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values)
        {
            throw new System.NotImplementedException();
        }

        Task<List<List<StatusChange>>> IOutApiClient.GetOrderStatusChanges(List<int> orderIds)
        {
            throw new NotImplementedException();
        }
    }
    public interface IOutApiClient {
        string Ck { get; }
        string Cs { get; }
        string Address { get; }

        /*
        public string Post(string op, string body);
        public string Put(string op, string body);
        public string Get(string op);
        public string Delete(string op);
        */
        void SetLogger(IMyLogger logger);

        Task<string> Get(string msg);
        
        Task<Dictionary<string, object>> UpdateProduct(string id, Dictionary<string, object> values);
        
        Task<List<Dictionary<string, object>>> GetProducts();
        Task<ProductResult> GetProductBySku(string sku);
        
        Task<Dictionary<string, object>> PostProduct(Dictionary<string, object> product);
        Task<Dictionary<string, object>> PostBaseVariableProduct(Dictionary<string, object> product);
        Task<List<Dictionary<string, object>>> GetVariableProducts();
        Task<string> DeleteProductBySku(string sku);
        Task<string> DeleteProduct(string id);



        Task<Dictionary<string, object>> PostVariation(string parent_id, Dictionary<string, object> variation);
        Task<Dictionary<string, object>> UpdateVariation(string product_id, string variation_id, Dictionary<string, object> values);
        Task<string> DeleteVariation(string parent_id, string variation_id);
        


        Task<List<Dictionary<string, object>>> GetAttributes();
        Task<List<Dictionary<string, object>>> GetAttributes(string productId);
        Task<Dictionary<string, object>> PostAttribute(Dictionary<string, object> attribute);
        Task<List<Dictionary<string, object>>> GetAttributeTerms(string attributeId);
        Task<Dictionary<string, object>> PostAttributeTerm(string attributeId, string attributeTerm);
        Task<List<Category>> GetCategories();
        Task<Category> PostCategory(string name);

        Task<string> MyGetOrder(string id);
        Task<List<string>> GetOrders(DateTime sinceDate);
        Task<List<string>> GetOrderDescriptions(DateTime sinceDate);
        Task<List<List<StatusChange>>> GetOrderStatusChanges(List<int> orderIds);



        //////////////////////////////////////////////////////////////////////////////
        // PLUGIN METHODS
        //////////////////////////////////////////////////////////////////////////////


        Task<string> GetKita(string query);
        Task<string> PutKita(string query, string body);


        Task<string> PostKita(string query, string body);

        Task<string> DeleteKita(string query);

    }

    public class ProductResult
    {
        public bool Success { get; set; }
        public Dictionary<string, object> Product { get; set; }
        public string ErrorMessage { get; set; }

        public static ProductResult SuccessResult(Dictionary<string, object> product)
        {
            return new ProductResult { Success = true, Product = product };
        }

        public static ProductResult FailureResult(string errorMessage)
        {
            return new ProductResult { Success = false, ErrorMessage = errorMessage };
        }
    }


    public class Category {
        public string id;
        public string name;
    }

    public class ProductValue {

        public ProductParameter param;
        public string value;
        public ProductValue(ProductParameter param, string value) {
            this.param = param;
            this.value = value;
        } 
    }

    public enum ProductParameter {
        GrossPrice,
        NetPrice,
        SalePrice,
        Name
    }
}
