using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class HallSeat
{
    public int SeatId { get; set; }

    public int HallId { get; set; }

    public int RowNumber { get; set; }

    public int SeatNumber { get; set; }

    public string? SeatType { get; set; }

    public decimal? PriceMultiplier { get; set; }

    public bool? IsActive { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
