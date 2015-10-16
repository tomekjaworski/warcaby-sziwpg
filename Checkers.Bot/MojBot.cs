using Checkers.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Checkers.Bot
{
    public class MojBot : IBotEngine
    {
        IBoard plansza;

        #region Informacje o autorze bota
        public string GetAuthor()
        {
            return "tutaj wpisz swoje imie i nazwisko";
        }

        public string GetDescripion()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Tutaj należy wpisać informację o bocie - krótki opis jego działania, zastosowanych algorytmów, itd...");
            sb.AppendLine("5-6 zdań będzie dobrą długością opisu");
            return sb.ToString();
        }

        public void Initialize()
        {
            /*
                TODO: Kod inicjujący bota
                Ten kod wykonywany jest raz, po starcie głownej aplikacji. 
             */
        }

        public void NewGame(PawnColor cpu_color, IBoard board)
        {
            /*
                TODO: Kod startowy nowej gry
                Kod uruchamiany jest każdorazowo podczas rozpoczynania nowej gry.
             */

            this.plansza = board;
        }

        public void MakeMove()
        {

            /*
              TODO: Kod ruchu
              tutaj bot wykonuje swój ruch
             */

            PawnType[,] tab = this.plansza.GetCheckboard();

            this.plansza.SelectPawn("G4");
            this.plansza.MoveSelectedPawnTo("F5");
        }


       
        #endregion
    }
}
