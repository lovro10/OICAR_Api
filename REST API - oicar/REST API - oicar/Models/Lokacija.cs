using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Lokacija
{
    public int Idlokacija { get; set; }

    public string Polaziste { get; set; } = null!;

    public string Odrediste { get; set; } = null!;

    public virtual ICollection<Oglasvoznja> Oglasvoznjas { get; set; } = new List<Oglasvoznja>();
}
