using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsDefault { get; set; }

    public virtual Product Product { get; set; } = null!;
}
