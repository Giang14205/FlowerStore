using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public int? VoucherId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal ShippingAmount { get; set; }

    public int PayMethodId { get; set; }

    public int OrderStatusId { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual PayMethod PayMethod { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
