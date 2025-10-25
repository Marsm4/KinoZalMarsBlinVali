using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class FinancialTransaction
{
    public int TransactionId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Description { get; set; }

    public int? EmployeeId { get; set; }

    public int? TicketId { get; set; }

    public DateTime? TransactionTime { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Ticket? Ticket { get; set; }
}
