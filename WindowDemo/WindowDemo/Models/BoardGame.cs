using System;
using System.Collections.Generic;

namespace WindowDemo.Models;

public partial class BoardGame
{
    public string? Title { get; set; }

    public string? Genre { get; set; }

    public int? NumPlayers { get; set; }

    public int? Age { get; set; }

    public string? Publisher { get; set; }

    public int? ProduktId { get; set; }

    // Navigationsproperty
    public virtual ProduktCategory Category { get; set; }
}
