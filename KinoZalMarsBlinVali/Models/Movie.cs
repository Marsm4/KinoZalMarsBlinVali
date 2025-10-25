using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Genre { get; set; }

    public int DurationMinutes { get; set; }

    public string? AgeRating { get; set; }

    public string? Director { get; set; }

    public string? CastText { get; set; }

    public string? Description { get; set; }

    public string? PosterPath { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
