using System;
using System.Collections.Generic;

namespace KinoZalMarsBlinVali.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public string ImageName { get; set; } = null!;

    public byte[]? ImageData { get; set; }

    public string? ImageType { get; set; }

    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public string? FilePath { get; set; }

    public DateTime? UploadedAt { get; set; }
}
