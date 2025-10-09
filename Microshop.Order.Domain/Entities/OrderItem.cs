namespace Microshop.Order.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public Guid ProductId { get; set; }
    
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }

    public decimal TotalPrice { get;private set; }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
        TotalPrice = Quantity * ProductPrice;
    }
}