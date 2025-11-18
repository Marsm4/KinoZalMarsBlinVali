using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public string? QuizDescription { get; set; }

    public int? MovieId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? BonusPoints { get; set; }

    public int? PassingScore { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
