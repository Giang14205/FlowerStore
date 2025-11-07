using System;
using System.Collections.Generic;

namespace FlowerShop.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? Status { get; set; }

    public string? Description { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();

    public virtual ICollection<ContactReply> ContactReplies { get; set; } = new List<ContactReply>();

    public virtual ICollection<FeedbackCustomer> FeedbackCustomers { get; set; } = new List<FeedbackCustomer>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
