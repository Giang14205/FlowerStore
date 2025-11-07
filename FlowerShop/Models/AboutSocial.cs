using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class AboutSocial
{
    public int SocialId { get; set; }

    public int AboutId { get; set; }

    public string? SocialPlatform { get; set; }

    public string Url { get; set; } = null!;

    public virtual AboutSection About { get; set; } = null!;
}
