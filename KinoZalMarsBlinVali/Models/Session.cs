using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KinoZalMarsBlinVali.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public int MovieId { get; set; }

    public int HallId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public decimal BasePrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();






    [NotMapped]
    public int AvailableSeats
    {
        get
        {
            try
            {
                var totalSeats = Hall?.TotalSeats ?? 0;
                var soldTickets = Tickets?.Count(t => t.Status == "sold" || t.Status == "used") ?? 0;
                return totalSeats - soldTickets;
            }
            catch
            {
                return 0;
            }
        }
    }
   
    [NotMapped]
    public string MovieTitle => Movie?.Title ?? "Неизвестно";

    [NotMapped]
    public string HallName => Hall?.HallName ?? "Неизвестно";
}
