﻿using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Korisnikvoznja
{
    public int Idkorisnikvoznja { get; set; }

    public int? Korisnikid { get; set; }

    public int? Oglasvoznjaid { get; set; }

    public string? Lokacijavozac { get; set; }

    public string? Lokacijaputnik { get; set; }

    public virtual Korisnik? Korisnik { get; set; }

    public virtual Oglasvoznja? Oglasvoznja { get; set; }
}
