﻿using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Uloga
{
    public int Iduloga { get; set; }

    public string Naziv { get; set; } = null!;

    public virtual ICollection<Korisnik> Korisniks { get; set; } = new List<Korisnik>();
}
