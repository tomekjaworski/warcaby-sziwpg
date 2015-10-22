using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{

    /// <summary>
    /// Klasa metod statycznych operacji na kolorach i typach pionów
    /// </summary>
    public static class Pawn
    {
        /// <summary>
        /// Porównanie dwóch pionów ze względu na ich kolor
        /// </summary>
        /// <param name="pc1">Kolor pionu pierwszego</param>
        /// <param name="pc2">Kolor pionu drugiego</param>
        /// <returns>Zwraca true jeśli kolory są zgodne</returns>
        static bool EqualColor(PawnColor pc1, PawnColor pc2)
        {
            return pc1 == pc2;
        }

        /// <summary>
        /// Porównanie dwóch pionów ze względu na ich kolor (z pominięciem typu)
        /// </summary>
        public static bool EqualColor(PawnColor pc, PawnType pt)
        {
            if (pc == PawnColor.Black && (pt == PawnType.BlackPawn || pt == PawnType.BlackQueen))
                return true;
            if (pc == PawnColor.White && (pt == PawnType.WhitePawn || pt == PawnType.WhiteQueen))
                return true;

            return false;
        }

        /// <summary>Sprawdza, czy typ reprezentuje normalny pion (dowolnego koloru)</summary>
        /// <param name="pt">Typ pionu</param>
        /// <returns>Zwraca true jeśli typ reprezentuje zwykły pion</returns>
        public static bool IsNormalPawn(PawnType pt)
        {
            return (pt == PawnType.BlackPawn || pt == PawnType.WhitePawn);
        }

        /// <summary>Sprawdza, czy typ reprezentuje damę (dowolnego koloru)</summary>
        /// <param name="pt">Typ pionu</param>
        /// <returns>Zwraca true jeśli typ reprezentuje damę</returns>
        public static bool IsQueenPawn(PawnType pt)
        {
            return (pt == PawnType.BlackQueen || pt == PawnType.WhiteQueen);
        }

        /// <summary>Pobiera kolor piona na podstawie jego typu</summary>
        /// <param name="pt">Typ piona</param>
        /// <returns>Kolor piona</returns>
        public static PawnColor GetColor(PawnType pt)
        {
            if (pt == PawnType.BlackPawn || pt == PawnType.BlackQueen)
                return PawnColor.Black;
            if (pt == PawnType.WhitePawn || pt == PawnType.WhiteQueen)
                return PawnColor.White;

            return PawnColor.None;
        }

        /// <summary>Określ kolor przeciwnika dla podanego koloru</summary>
        /// <param name="pc">Kolor</param>
        /// <returns>Kolor przeciwnika</returns>
        public static PawnColor GetOpponentColor(PawnColor pc)
        {
            if (pc == PawnColor.Black)
                return PawnColor.White;
            if (pc == PawnColor.White)
                return PawnColor.Black;
            return PawnColor.None;
        }


        public static string NormalizeFieldAddress(string field_addr)
        {
            if (String.IsNullOrEmpty(field_addr))
                throw new GameException(string.Format("Niepoprawne współrzędne piona (1) - {0}", field_addr));
            if (field_addr.Length != 2)
                throw new GameException(string.Format("Niepoprawne współrzędne piona (2) - {0}", field_addr));

            field_addr = field_addr.ToUpper();
            if (!((field_addr[0] >= 'A') && (field_addr[0] <= 'H') ||
                (field_addr[0] >= '1') && (field_addr[0] <= '8')))
                throw new GameException(string.Format("Niepoprawne współrzędne piona (3) - {0}", field_addr));
            if (!((field_addr[1] >= '1') && (field_addr[1] <= '8')))
                throw new GameException(string.Format("Niepoprawne współrzędne piona (4) - {0}", field_addr));

            return field_addr;
        }

        /// <summary>Zamienia adres pola w postaci tekstowej na strukturę System.Drawing.Point</summary>
        /// <param name="field_address">Tekstowy adres pola w postaci <b>A1-H8</b> lub <b>11-88</b></param>
        /// <returns>Współrzędne pola X=0..7; Y=0..7</returns>
        public static Point FieldAddressToPoint(string field_address)
        {
            field_address = NormalizeFieldAddress(field_address);

            int col = field_address[1] - '1';
            int row = field_address[0] >= 'A' ? field_address[0] - 'A' : field_address[0] - '1';

            return new Point(col, row);
        }

        /// <summary>Zamienia tablicę adreów pól w postaci tekstowej na tablicę struktur System.Drawing.Point</summary>
        /// <param name="field_addresses">Tablica tekstowych adresów pól w postaci <b>A1-H8</b> lub <b>11-88</b></param>
        /// <returns>Tablica współrzędnych pól X=0..7; Y=0..7</returns>
        public static Point[] FieldAddressToPoint(String[] field_addresses)
        {
            if (field_addresses == null)
                return new Point[0];

            return Array.ConvertAll<String, Point>(field_addresses, new Converter<String, Point>(Pawn.FieldAddressToPoint));
        }

        /// <summary>Zamienia współrzędną numeryczną pola (obiekt System.Drawing.Point) na postać tekstową <b>A1-H8</b></summary>
        /// <param name="p">Współrzędne pola X=0..7; Y=0..7</param>
        /// <returns>Tekstowa reprezentacja współrzędnych pola</returns>
        public static string PointToFieldAddress(Point p)
        {
            String s = string.Format("{0}{1}", (char)('A' + p.Y), (1 + p.X).ToString());
            return s;
        }

        /// <summary>Zamienia współrzędną numeryczną pola (obiekt System.Drawing.Point) na postać tekstową <b>A1-H8</b></summary>
        /// <param name="points">Tablica współrzędnych pól X=0..7; Y=0..7</param>
        /// <returns>Tablica tekstowych reprezentacji współrzędnych pól</returns>
        public static string[] PointToFieldAddress(Point[] points)
        {
            if (points == null)
                return new string[0];

            return Array.ConvertAll<Point, String>(points, new Converter<Point, String>(Pawn.PointToFieldAddress));
        }


        public static bool IsNone(PawnType pt)
        {
            return pt == PawnType.None;
        }

        public static bool IsNone(PawnColor pc)
        {
            return pc == PawnColor.None;
        }


        public static bool InBound(Point p)
        {
            if (p.X < 0 || p.X > 7)
                return false;
            if (p.Y < 0 || p.Y > 7)
                return false;
            return true;
        }

        /// <summary>Zwróć damę jako typ piona dla podanego koloru</summary>
        /// <param name="pc">Kolor</param>
        /// <returns>Dama w danym kolorze</returns>
        public static PawnType GetQueenByColor(PawnColor pc)
        {
            if (pc == PawnColor.Black)
                return PawnType.BlackQueen;
            if (pc == PawnColor.White)
                return PawnType.WhiteQueen;
            return PawnType.None;
        }

        /// <summary>Zwróć piona  dla podanego koloru</summary>
        /// <param name="pc">Kolor</param>
        /// <returns>Pion w danym kolorze</returns>
        public static PawnType GetPawnByColor(PawnColor pc)
        {
            if (pc == PawnColor.Black)
                return PawnType.BlackPawn;
            if (pc == PawnColor.White)
                return PawnType.WhitePawn;
            return PawnType.None;
        }
    }
}
