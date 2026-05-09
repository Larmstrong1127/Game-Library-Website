using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CSC595_Week04.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UniqueProductNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var productService = (IProductService)validationContext.GetService(typeof(IProductService));
            var products = productService.GetProducts();

            var productName = value as string;
            if (products.Any(p => p.Name == productName))
            {
                return new ValidationResult("Product name must be unique.");
            }

            return ValidationResult.Success;
        }
    }
}


