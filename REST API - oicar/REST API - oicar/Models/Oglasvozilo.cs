using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Oglasvozilo
{
    public int Idoglasvozilo { get; set; }

    public int? Voziloid { get; set; }

    public DateTime DatumPocetkaRezervacije { get; set; }

    public DateTime DatumZavrsetkaRezervacije { get; set; }

    public int? Korisnikid { get; set; }

    public virtual Korisnik? Korisnik { get; set; }

    public virtual Vozilo? Vozilo { get; set; }
}
