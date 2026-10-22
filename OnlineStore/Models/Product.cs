using System;
using System.Collections.Generic;

namespace OnlineStore.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public int IdCategory { get; set; }

    public string NameProduct { get; set; } = null!;

    public decimal PriceProduct { get; set; }

    public int QuantityProduct { get; set; }

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
