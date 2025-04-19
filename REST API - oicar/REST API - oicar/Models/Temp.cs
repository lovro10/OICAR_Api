using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Temp
{
    public int Idkorisnik { get; set; }

    public string Ime { get; set; } = null!;

    public bool? Isconfirmed { get; set; }
}
