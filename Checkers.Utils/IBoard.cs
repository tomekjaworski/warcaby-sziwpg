﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
    public interface IBoard
    {

        /// <summary>
        /// Wybierz pion na podstawie adresu pola
        /// </summary>
        /// <param name="field_address">Adres pola z pionem (np. "C3")</param>
        /// <returns>PawnType.None jeśli na polu nie było piona lub nie udało się go wybrać (kolor przeciwnika)</returns>
        PawnType SelectPawn(string field_address);

        /// <summary>
        /// Usuń zaznaczenie pola z pionem (jeśli takie było)
        /// </summary>
        void DeselectPawn();

        /// <summary>Pobierz typ piona z pola o podanym adresie</summary>
        /// <param name="field_address">Adres pola z pionem (np. "C3")</param>
        /// <returns>Typ piona na polu o podanym adresie</returns>
        PawnType GetPawn(string field_address);

        /// <summary>
        /// Zwróć typ wcześniej wybranego piona, metodą <b>IBoard.SelectPawn()</b>
        /// </summary>
        /// <returns>Typ wybranego piona lub PawnType.None</returns>
        PawnType GetSelectedPawn();

        /// <summary>
        /// Sprawdza, czy wybrany pion/damkę można przesunąć do nowej lokalizacji. Metoda ta uwzględnia zarówno damkę jak i opcje bicia pionów przeciwnika
        /// </summary>
        /// <param name="field_address">Adres docelowego pola (np. "C3")</param>
        /// <returns>True jeśli można przesunąć pion/damkę</returns>
        bool CanMoveSelectedPawnTo(string field_address);

        /// <summary>
        /// Przesuń pion/damke na wybrane pole. Jesli po drodze znajdują się piony przeciwnika i można je usunać z planszy to zrób to.
        /// </summary>
        /// <param name="field_address">Adres docelowego pola (np. "C3")</param>
        /// <returns>Krótka informacja o zmianie stanu planszy podczas wykonywania ruchu <seealso cref="PawnMoveResult"/></returns>
        PawnMoveResult MoveSelectedPawnTo(string field_address);

        /// <summary>Pobiera zawartość całej planszy (tablica 8x8)<br/>Pierwsza współrzędna - wiersze<br/>Druga współrzędna - kolumny</summary>
        /// <returns></returns>
        PawnType[,] GetCheckboard();

        /// <summary>Pobierz listę współrzędnych pól na które pion, spod adresu <i>field_address</i>, może się przesunąć <b>BEZ</b> zbijania pionów przeciwnika.</summary>
        /// <param name="field_address">Adres piona w postaci tekstowej (np. "A1")</param>
        /// <seealso cref="IBoard.GetMovementCoordinates(string)"/>
        /// <seealso cref="Pawn.PointToFieldAddress(Point)"/>
        /// <returns>Lista współrzędnych w postaci adresów pól String[]</returns>
        string[] GetMovementFields(string field_address);
        
        /// <summary>Pobierz listę współrzędnych pól dla hipotetycznego piona spod adresu <i>field_address</i>. Lista uwzględnia przesunięcia piona <b>BEZ</b> zbijania pionów przeciwnika.</summary>
        /// <param name="field_address">Adres piona w postaci tekstowej (np. "A1")</param>
        /// <param name="hypotetical_pawn_type">Typ piona dla którego metoda ma określić listę ruchów</param>
        /// <seealso cref="IBoard.GetMovementCoordinates(string)"/>
        /// <seealso cref="Pawn.PointToFieldAddress(Point)"/>
        /// <returns>Lista współrzędnych w postaci adresów pól String[]</returns>
        string[] GetMovementFields(string field_address, PawnType hypotetical_pawn_type);

        /// <summary>Pobierz listę współrzędnych pól na które pion, spod adresu <i>field_address</i>, może się przesunąć <b>POD WARUNKIEM</b> zbicia choć jednego piona przeciwnika.</summary>
        /// <param name="field_address">Adres piona w postaci tekstowej (np. "A1")</param>
        /// <seealso cref="IBoard.GetCaptureCoordinates(string)"/>
        /// <seealso cref="Pawn.PointToFieldAddress(Point)"/>
        string[] GetCaptureFields(string field_address);

        /// <summary>Pobierz listę współrzędnych pól dla hipotetycznego piona spod adresu<i>field_address</i>, który może się przesunąć <b>POD WARUNKIEM</b> zbicia choć jednego piona przeciwnika.</summary>
        /// <param name="field_address">Adres piona w postaci tekstowej (np. "A1")</param>
        /// <param name="hypotetical_pawn_type">Typ piona dla którego metoda ma określić listę ruchów</param>
        /// <seealso cref="IBoard.GetMovementCoordinates(string)"/>
        /// <seealso cref="Pawn.PointToFieldAddress(Point)"/>
        /// <returns>Lista współrzędnych w postaci adresów pól String[]</returns>
        string[] GetCaptureFields(string field_address, PawnType hypotetical_pawn_type);




        /// <summary>Sprawdź możliwość wykonania bicia</summary>
        /// <param name="field_address">Adres piona bijącego (np. "A1")</param>
        /// <returns>True - jeśli pion spod podanego adresu może wykonać bicie</returns>
        bool IsCaptureAvailable(string field_address);



        /// <summary>
        /// Wyświetl tekst w oknie logów (dla gracza)
        /// </summary>
        /// <param name="msg">Treść komunikatu</param>
        void AddPlayerLog(string msg);

        /// <summary>
        /// Wyświetl tekst w oknie logów (tekst systemowy)
        /// </summary>
        /// <param name="msg">Treść komunikatu</param>
        void AddSystemLog(string msg);

    }
}
