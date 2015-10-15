﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{

    /// <summary>
    /// Klasa metod statycznych operacji na kolorach i typach pionów
    /// </summary>
    public class Pawn
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


        /// <summary>Zamienia adres pola w postaci tekstowej na strukturę System.Drawing.Point</summary>
        /// <param name="field_address">Tekstowy adres pola w postaci <b>A1-H8</b> lub <b>11-88</b></param>
        /// <returns>Współrzędne pola X=0..7; Y=0..7</returns>
        public static Point FieldAddressToPoint(string field_address)
        {
            if (String.IsNullOrEmpty(field_address))
                throw new GameException("Niepoprawne współrzędne piona (1)");
            if (field_address.Length != 2)
                throw new GameException("Niepoprawne współrzędne piona (2)");

            field_address = field_address.ToUpper();
            if (!((field_address[0] >= 'A') && (field_address[0] <= 'H') ||
                (field_address[0] >= '1') && (field_address[0] <= '8')))
                throw new GameException("Niepoprawne współrzędne piona (3)");
            if (!((field_address[1] >= '1') && (field_address[1] <= '8')))
                throw new GameException("Niepoprawne współrzędne piona (4)");

            int col = field_address[1] - '1';
            int row = field_address[0] >= 'A' ? field_address[0] - 'A' : field_address[0] - '1';

            return new Point(col, row);
        }

        /// <summary>Zamienia współrzędną numeryczną pola (obiekt System.Drawing.Point) na postać tekstową <b>A1-H8</b></summary>
        /// <param name="p">Współrzędne pola X=0..7; Y=0..7</param>
        /// <returns>Tekstowa reprezentacja współrzędnych pola</returns>
        public static string PointToFieldAddress(Point p)
        {
            String s = string.Format("{0}{1}", (char)('A' + p.Y), (1 + p.X).ToString());
            return s;
        }
    }
}