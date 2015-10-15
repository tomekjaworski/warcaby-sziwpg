using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
    /// <summary>
    /// Typ opisujący typ piona
    /// </summary>
    public enum PawnType
    {
        /// <summary>Brak piona</summary>
        None = 0,

        /// <summary>Czarny pion (zwykły)</summary>
        BlackPawn,

        /// <summary>Biały pion (zwykły)</summary>
        WhitePawn,
        
        /// <summary>Czarna królowa (dama)</summary>
        BlackQueen,
        
        /// <summary>Biała królowa (dama)</summary>
        WhiteQueen
    }

}
