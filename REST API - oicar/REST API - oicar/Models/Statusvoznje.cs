﻿using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Statusvoznje
{
    public int Idstatusvoznje { get; set; }

    public string Naziv { get; set; } = null!;

    public virtual ICollection<Oglasvoznja> Oglasvoznjas { get; set; } = new List<Oglasvoznja>();
}
