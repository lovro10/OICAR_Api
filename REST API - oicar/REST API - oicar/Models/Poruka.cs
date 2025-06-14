﻿using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Poruka
{
    public int Idporuka { get; set; }

    public int? Oglasvoznjaid { get; set; }

    public int? Putnikid { get; set; }

    public int? Vozacid { get; set; }

    public string Content { get; set; } = null!;

    public virtual Oglasvoznja? Oglasvoznja { get; set; }

    public virtual Korisnik? Putnik { get; set; }

    public virtual Korisnik? Vozac { get; set; }
}
