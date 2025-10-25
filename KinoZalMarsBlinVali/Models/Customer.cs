using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? BonusPoints { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Password { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
