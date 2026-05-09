using System.Collections.Generic;
using System.Linq;

namespace CSC595_Week04.Models
{
    public class FakeProductService : IProductService
    {
        private readonly List<Product> _products;

        public FakeProductService()
        {
            _products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.99M },
                new Product { Id = 2, Name = "Product 2", Price = 15.99M },
                new Product { Id = 3, Name = "Product 3", Price = 12.49M },
                new Product { Id = 4, Name = "Product 4", Price = 9.99M },
                new Product { Id = 5, Name = "Product 5", Price = 7.99M }
            };
        }

        public IEnumerable<Product> GetProducts()
        {
            return _products;
        }

        public Product GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public void AddProduct(Product product)
        {
            _products.Add(product);
        }

        public void UpdateProduct(Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (product != null)
            {
                product.Name = updatedProduct.Name;
                product.Price = updatedProduct.Price;
                
            }
        }

        public void DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
            }
        }
    }
}