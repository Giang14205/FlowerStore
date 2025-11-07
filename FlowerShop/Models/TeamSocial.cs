using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class TeamSocial
{
    public int SocialId { get; set; }

    public int TeamMemberId { get; set; }

    public string? SocialPlatform { get; set; }

    public string Url { get; set; } = null!;

    public virtual TeamMember TeamMember { get; set; } = null!;
}
