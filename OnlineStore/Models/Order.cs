using System;
using System.Collections.Generic;

namespace OnlineStore.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public int IdUser { get; set; }

    public DateTime DateOrder { get; set; }

    public int IdOrderStatus { get; set; }

    public string DeliveryAddress { get; set; } = null!;

    public virtual OrderStatus IdOrderStatusNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
