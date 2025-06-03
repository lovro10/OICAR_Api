using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Porukavozilo
{
    public int Idporuka { get; set; }

    public int? Korisnikvoziloid { get; set; }

    public int? Putnikid { get; set; }

    public int? Vozacid { get; set; }

    public string Content { get; set; } = null!;

    public virtual Korisnikvozilo? Korisnikvozilo { get; set; }

    public virtual Korisnik? Putnik { get; set; }

    public virtual Korisnik? Vozac { get; set; }
}
