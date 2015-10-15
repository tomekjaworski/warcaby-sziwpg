using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
   public  interface IBotEngine
    {

        /// <summary>Pobiera informację o autorze bota</summary>
        /// <returns>Imię i nazwisko autora</returns>
        string GetAuthor();
        
        /// <summary>Pobiera opis działania bota</summary>
        /// <returns>Opis działania bota</returns>
        string GetDescripion();


        /// <summary>
        /// Inicjowanie bota. Metoda wykonywana po starcie programu szachowego.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Przygotowanie bota do nowej gry. Metoda jest wykonywana po wszystkich przygotowaniach (m.in. po ustawieniu pionów na planszy)
        /// </summary>
        /// <param name="cpu_color">Kolor jakim gra komputer/bot</param>
        /// <param name="board">Interfejs do pracy z planszą</param>
        void NewGame(PawnColor cpu_color, IBoard board);

        /// <summary>
        /// Ruch bota. Metoda realizuje jeden ruch bota (pionem lub damą)
        /// </summary>
        void MakeMove();

    }
}
