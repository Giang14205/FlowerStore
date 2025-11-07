using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class FeedbackCustomer
{
    public int FeedbackCustomerId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public virtual User User { get; set; } = null!;
}
