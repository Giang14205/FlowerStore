using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public int ProductCategoryId { get; set; }

    public int UserId { get; set; }

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedBlog { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
