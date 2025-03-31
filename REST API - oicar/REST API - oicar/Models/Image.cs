using System;
using System.Collections.Generic;

namespace REST_API___oicar.Models;

public partial class Image
{
    public int Idimage { get; set; }

    public string Name { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public virtual ICollection<Korisnik> KorisnikImagelices { get; set; } = new List<Korisnik>();

    public virtual ICollection<Korisnik> KorisnikImageosobnas { get; set; } = new List<Korisnik>();

    public virtual ICollection<Korisnik> KorisnikImagevozackas { get; set; } = new List<Korisnik>();

    public virtual ICollection<Vozilo> Vozilos { get; set; } = new List<Vozilo>();
}
