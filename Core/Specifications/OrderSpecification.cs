using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderSpecification : BaseSpecifications<Order>
{
    public OrderSpecification(string email) : base(x => x.BuyerEmail == email)
    {

    }

    public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
    {

    }
}
