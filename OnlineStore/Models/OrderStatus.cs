using System;
using System.Collections.Generic;

namespace OnlineStore.Models;

public partial class OrderStatus
{
    public int IdOrderStatus { get; set; }

    public string? NameOrderStatus { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
