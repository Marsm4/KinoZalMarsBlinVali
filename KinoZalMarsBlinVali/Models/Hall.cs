using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Hall
{
    public int HallId { get; set; }

    public string HallName { get; set; } = null!;

    public int TotalSeats { get; set; }

    public int SeatRows { get; set; }

    public int SeatColumns { get; set; }

    public string? HallType { get; set; }

    public string? LayoutSchemaPath { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<HallSeat> HallSeats { get; set; } = new List<HallSeat>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
