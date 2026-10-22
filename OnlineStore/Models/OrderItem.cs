using System;
using System.Collections.Generic;

namespace OnlineStore.Models;

public partial class OrderItem
{
    public int IdOrderItem { get; set; }

    public int IdOrder { get; set; }

    public int IdProduct { get; set; }

    public int QuantityOrderItem { get; set; }

    public decimal PriceOrderItem { get; set; }

    public virtual Order IdOrderNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;
}
