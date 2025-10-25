using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class TicketService
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int ServiceId { get; set; }

    public int? Quantity { get; set; }

    public virtual AdditionalService Service { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
