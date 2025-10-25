using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class AdditionalService
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<TicketService> TicketServices { get; set; } = new List<TicketService>();
}
