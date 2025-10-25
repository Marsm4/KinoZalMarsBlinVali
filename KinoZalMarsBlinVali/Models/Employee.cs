using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models
{

    public partial class Employee
    {
        public int EmployeeId { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Position { get; set; } = null!;

        public string Role { get; set; } = null!;

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}