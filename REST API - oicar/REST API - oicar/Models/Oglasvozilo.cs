﻿using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Oglasvozilo
{
    public int Idoglasvozilo { get; set; }

    public int? Voziloid { get; set; }

    public DateTime DatumPocetkaRezervacije { get; set; }

    public DateTime DatumZavrsetkaRezervacije { get; set; }

    public virtual ICollection<Korisnikvozilo> Korisnikvozilos { get; set; } = new List<Korisnikvozilo>();

    public virtual Vozilo? Vozilo { get; set; }
}
