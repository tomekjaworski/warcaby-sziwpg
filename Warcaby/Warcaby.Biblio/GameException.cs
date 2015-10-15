using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checkers.Utils
{

    /// <summary>
    /// Klasa opisująca wyjątek krytyczny, mogący wystąpić podczas gry (np. próba odczytu piona z pola o współrzędnych innych niż poprawne)
    /// </summary>
    [Serializable]
    public class GameException : Exception
    {
        public GameException() { }
        public GameException(string message) : base(message) { }
        public GameException(string message, Exception inner) : base(message, inner) { }
        protected GameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

}
