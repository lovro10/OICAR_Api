using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Imagetype
{
    public int Idimagetype { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
