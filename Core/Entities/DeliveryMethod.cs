namespace Core.Entities;

public class DeliveryMethod : BaseEntity
{

    public required string ShortName { get; set; }
    public required string DeliveryTime { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
}
