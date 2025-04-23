using Core.Entities;

namespace Core.Specifications;

public class ProductSpecification : BaseSpecifications<Product>
{
    /* old code
    public ProductSpecification(string? brand, string? type, string? sort) : base(x => 
        (string.IsNullOrWhiteSpace(brand) || x.Brand == brand) &&
        (string.IsNullOrWhiteSpace(type) || x.Type == type))
     {
        switch (sort)
        //...
    */
    public ProductSpecification(ProductSpecParams specParams) : base(x => 
        (specParams.Brands.Any() || specParams.Brands.Contains(x.Brand) ) &&
        (specParams.Types.Any() || specParams.Types.Contains(x.Type)))
    {
        // switch (sort)
        switch (specParams.Sort)
        {
            case "priceAsc":
                AddOrderBy(x => x.Price);
                break;
            case "priceDesc":
                AddOrderByDescending(x => x.Price);
                break;
            default:
                AddOrderBy(x => x.Name);
                break;
        }
    }    
}
