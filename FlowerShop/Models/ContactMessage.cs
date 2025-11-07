using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class ContactMessage
{
    public int ContactMessageId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Message { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<ContactReply> ContactReplies { get; set; } = new List<ContactReply>();

    public virtual User? User { get; set; }
}
