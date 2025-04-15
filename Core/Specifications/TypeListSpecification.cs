using Core.Entities;

namespace Core.Specifications;

public class TypeListSpecification : BaseSpecifications<Product, string>
{
    public TypeListSpecification()
    {   
        AddSelect(x => x.Type);
        AppyDistinct();
    }
}
