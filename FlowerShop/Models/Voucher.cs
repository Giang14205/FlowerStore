using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string VoucherName { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal MinOrder { get; set; }

    public decimal? MaxDiscount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
