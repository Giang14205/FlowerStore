using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class ContactReply
{
    public int ReplyId { get; set; }

    public int ContactMessageId { get; set; }

    public int UserId { get; set; }

    public string MessageReply { get; set; } = null!;

    public virtual ContactMessage ContactMessage { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
