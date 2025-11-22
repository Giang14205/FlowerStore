using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class ProductCategory
{
    public int ProductCategoryId { get; set; }

    public string? ProductCategoryName { get; set; }

    public string? ProductCategoryDescription { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Alias { get; set; }

    public int? Position { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
