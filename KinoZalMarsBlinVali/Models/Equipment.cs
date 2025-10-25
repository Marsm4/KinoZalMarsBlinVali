using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public int HallId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public string? Model { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public DateOnly? LastMaintenanceDate { get; set; }

    public DateOnly? NextMaintenanceDate { get; set; }

    public string? Status { get; set; }

    public string? ImagePath { get; set; }

    public virtual Hall Hall { get; set; } = null!;
}
