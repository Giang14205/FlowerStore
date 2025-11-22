using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int? SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? Alias { get; set; }

    public virtual ICollection<Menu> InverseParent { get; set; } = new List<Menu>();

    public virtual Menu? Parent { get; set; }
}
