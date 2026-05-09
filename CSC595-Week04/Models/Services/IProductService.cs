using System.Collections.Generic;

namespace CSC595_Week04.Models
{
    public interface IProductService
    {
        IEnumerable<Product> GetProducts();
        Product GetProductById(int id);
        void AddProduct(Product product);
        void UpdateProduct(Product updatedProduct);
        void DeleteProduct(int id);
    }
}
