using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int SessionId { get; set; }

    public int SeatId { get; set; }

    public int? CustomerId { get; set; }

    public int TicketTypeId { get; set; }

    public int? EmployeeId { get; set; }

    public decimal FinalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? ReservationTime { get; set; }

    public DateTime? ReservationExpires { get; set; }

    public DateTime? PurchaseTime { get; set; }

    public string? QrCodeData { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

    public virtual HallSeat Seat { get; set; } = null!;

    public virtual Session Session { get; set; } = null!;

    public virtual ICollection<TicketService> TicketServices { get; set; } = new List<TicketService>();

    public virtual TicketType TicketType { get; set; } = null!;
}
