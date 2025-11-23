using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? ProductCategoryId { get; set; }

    public decimal ProductPrice { get; set; }

    public decimal? DiscountPrice { get; set; }

    public int? Stock { get; set; }

    public string? ProductDescription { get; set; }

    public int? ManufacturerId { get; set; }

    public int? PriceSale { get; set; }

    public string? Alias { get; set; }

    public bool IsNew { get; set; }

    public bool IsBestSeller { get; set; }

    public bool IsActive { get; set; }

    public string? ImagePopular { get; set; }

    public int? Star { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Color> Colors { get; set; } = new List<Color>();

    public virtual ICollection<FeedbackCustomer> FeedbackCustomers { get; set; } = new List<FeedbackCustomer>();

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ProductCategory? ProductCategory { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

    public virtual ICollection<Color> ColorsNavigation { get; set; } = new List<Color>();
}
