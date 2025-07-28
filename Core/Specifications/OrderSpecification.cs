using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderSpecification : BaseSpecifications<Order>
{
    public OrderSpecification(string email) : base(x => x.BuyerEmail == email)
    {
        AddInclude(x => x.OrderItems);
        AddInclude(x => x.DeliveryMethod);
        AddOrderByDescending(x => x.OrderDate);
    }

    public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod");
    }

    public OrderSpecification(string PaymentIntentId, bool isPaymentIntent) :
        base(x => x.PaymentIntentId == PaymentIntentId)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod");
    }
}
