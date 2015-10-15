using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
    public static class PointExt
    {
        /// <summary>
        /// Odejmuje punkt p2 od punktu p1 (po współrzędnych)
        /// </summary>
        /// <param name="p1">Lewa strona</param>
        /// <param name="p2">Prawa strona</param>
        /// <returns>Różnica</returns>
        public static Point Subtract(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// Określa punkt środkowy dla dwóch punktów skrajnych
        /// </summary>
        /// <param name="p1">Pierwszy punkt skrajny</param>
        /// <param name="p2">Drugi punkt skrajny</param>
        /// <returns>Punkt środkowy</returns>
        public static Point Midpoint(this Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }


        public static Point Add(this Point p, Point p1)
        {
            return new Point(p.X + p1.X, p.Y + p1.Y);
        }

        public static Point Add(this Point p, int dx, int dy)
        {
            return new Point(p.X + dx, p.Y + dy);
        }
    }
}
