using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class FeedbackCustomer
{
    public int FeedbackCustomerId { get; set; }

    public string Content { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public int? Star { get; set; }

    public bool? IsActive { get; set; }

    public int? ProductId { get; set; }

    public string? Ten { get; set; }

    public string? Email { get; set; }

    public virtual Product? Product { get; set; }
}
