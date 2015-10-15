using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{
    public interface IBoard
    {

        /// <summary>
        /// Wybierz pion na podstawie adresu pola
        /// </summary>
        /// <param name="field_address">Adres pola z pionem</param>
        /// <returns>PawnType.None jeśli na polu nie było piona lub nie udało się go wybrać (kolor przeciwnika)</returns>
        PawnType SelectPawn(string field_address);

        /// <summary>
        /// Usuń zaznaczenie pola z pionem (jeśli takie było)
        /// </summary>
        void DeselectPawn();

        /// <summary>Pobierz typ piona z pola o podanym adresie</summary>
        /// <param name="field_address">Adres pola z pionem</param>
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
        /// <param name="field_address">Adres docelowego pola</param>
        /// <returns>True jeśli można przesunąć pion/damkę</returns>
        bool CanMoveSelectedPawnTo(string field_address);

        /// <summary>
        /// Przesuń pion/damke na wybrane pole. Jesli po drodze znajdują się piony przeciwnika i można je usunać z planszy to zrób to.
        /// </summary>
        /// <param name="field_address">Adres docelowego pola</param>
        /// <returns>True jeśli udało się wykonać ruch</returns>
        bool MoveSelectedPawnTo(string field_address);


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
