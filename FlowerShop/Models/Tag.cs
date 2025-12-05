using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public string TagName { get; set; } = null!;

    public int? BlogId { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
