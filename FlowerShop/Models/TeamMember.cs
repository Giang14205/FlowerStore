using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class TeamMember
{
    public int TeamMemberId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Position { get; set; }

    public string? AvatarUrl { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<TeamSocial> TeamSocials { get; set; } = new List<TeamSocial>();

    public virtual User User { get; set; } = null!;
}
