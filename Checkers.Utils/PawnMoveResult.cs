using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
    /// <summary>
    /// Typ opisujący wynik metody MoveSelectedPawnTo(string).
    /// </summary>
    public enum PawnMoveResult
    {
        /// <summary>Brak ruchu (piona nie dało się ruszyć zgodnie z założeniami)</summary>
        Nothing,
        /// <summary>Zwykły ruch pionem o jedno pole lub damą o kilka pól. Bez bicia</summary>
        NormalMove,

        /// <summary>Ruch bijący piona (tylko jedno bicie) lub damy (jedno lub więcej bić)</summary>
        Capture,


        Invalid,
    }

}
