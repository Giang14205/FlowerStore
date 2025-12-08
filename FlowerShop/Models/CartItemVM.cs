namespace FlowerShop.Models
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Model gốc của bạn không có cái này
        public string Image { get; set; } // Model gốc cũng thiếu cái này
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
