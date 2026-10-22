namespace OnlineStore.Models
{
    // Обычный класс с данными, как модели в AutoService.
    public class CartItem
    {
        public int IdProduct { get; set; }
        public string NameProduct { get; set; } = "";
        public decimal PriceProduct { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get { return PriceProduct * Quantity; } }
    }
}
