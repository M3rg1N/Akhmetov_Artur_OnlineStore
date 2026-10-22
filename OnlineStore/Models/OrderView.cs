namespace OnlineStore.Models
{
    // Данные, которые отображаются в списках заказов.
    public class OrderView
    {
        public int IdOrder { get; set; }
        public string Login { get; set; } = "";
        public DateTime DateOrder { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public int IdOrderStatus { get; set; }
    }
}
