using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class PayMethod
{
    public int PayMethodId { get; set; }

    public string? PayMethodName { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
