using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class AboutSection
{
    public int AboutId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public string? ListImageUrl { get; set; }

    public virtual ICollection<AboutSocial> AboutSocials { get; set; } = new List<AboutSocial>();
}
