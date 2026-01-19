namespace PZ5Shop.ViewModels
{
    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }
        public decimal TotalPrice => PriceAtOrder * Quantity;
    }
}
