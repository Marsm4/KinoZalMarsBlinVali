using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class QuizAttempt
{
    public int AttemptId { get; set; }
    public int CustomerId { get; set; }
    public int QuizId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? ScorePercent { get; set; }
    public int? EarnedPoints { get; set; }
    public bool PointsAwarded { get; set; } // Измените на bool вместо bool?
    public int? CorrectAnswers { get; set; }
    public int? TotalQuestions { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Quiz Quiz { get; set; } = null!;
}