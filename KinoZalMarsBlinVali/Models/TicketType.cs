using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class TicketType
{
    public int TypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal? DiscountPercent { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
