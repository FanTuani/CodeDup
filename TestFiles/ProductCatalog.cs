using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce
{
    public class ProductCatalog
    {
        private List<Product> products;
        
        public ProductCatalog()
        {
            products = new List<Product>();
        }
        
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
                
            products.Add(product);
        }
        
        public Product FindProductById(int id)
        {
            return products.FirstOrDefault(p => p.Id == id);
        }
        
        public List<Product> GetAllProducts()
        {
            return new List<Product>(products);
        }
        
        public void RemoveProduct(int id)
        {
            var product = FindProductById(id);
            if (product != null)
            {
                products.Remove(product);
            }
        }
        
        public int GetProductCount()
        {
            return products.Count;
        }
    }
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        
        public Product(int id, string name, decimal price, string description)
        {
            Id = id;
            Name = name;
            Price = price;
            Description = description;
        }
    }
}
