using System;
using Core.Entities;

namespace Core.Specifications;

public class ProductSpecification : BaseSpecifications<Product>
{
    // traditional constructor 
    public ProductSpecification(string? brand, string? type) : base(x => 
        (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
        (string.IsNullOrWhiteSpace(type) || x.Type == type))
    {

    }    
}
