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
        PawnColor moj_color;

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
            this.moj_color = cpu_color;
        }

        public void MakeMove()
        {

            /*
              TODO: Kod ruchu
              tutaj bot wykonuje swój ruch
             */

            // przykład: Poniższy kod pokazuje, jak zbudować listę
            // tekstowych współrzędnych wszystkich pionów bota oraz człowieka (przeciwnika)
            PawnType[,] tab = this.plansza.GetCheckboard();

            List<Point> my_pawns = new List<Point>(); // moje piony (bota)
            List<Point> human_pawns = new List<Point>(); // piony człowieka
            for (int r = 0; r < 7; r++)
                for (int c = 0; c < 7; r++)
                {
                    if (Pawn.GetColor(tab[r, c]) == this.moj_color)
                        my_pawns.Add(new Point(c, r));
                    if (Pawn.GetColor(tab[r, c]) == Pawn.GetOpponentColor(this.moj_color))
                        human_pawns.Add(new Point(c, r));
                }
            string[] my_fields = Pawn.PointToFieldAddress(my_pawns.ToArray());
            string[] human_fields = Pawn.PointToFieldAddress(human_pawns.ToArray());


            this.plansza.SelectPawn("G4");
            this.plansza.MoveSelectedPawnTo("F5");
        }


       
        #endregion
    }
}
