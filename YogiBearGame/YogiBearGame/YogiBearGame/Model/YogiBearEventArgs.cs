using System;
using System.Collections.Generic;
using System.Text;

namespace YogiBearGame.Model
{
    public class YogiBearEventArgs
    {
        private Int32 _gameTime;
        private Int32 _gameTable;
        private Boolean _isWon;

        public Int32 GameTable { get { return _gameTable; } }

        /// <summary>
        /// Játékidő lekérdezése.
        /// </summary>
        public Int32 GameTime { get { return _gameTime; } }

        /// <summary>
        /// Győzelem lekérdezése.
        /// </summary>
        public Boolean IsWon { get { return _isWon; } }

        /// <summary>
        /// Sudoku eseményargumentum példányosítása.
        /// </summary>
        /// <param name="isWon">Győzelem lekérdezése.</param>
        /// <param name="gameTable">Tábla.</param>
        /// <param name="gameTime">Játékidő.</param>
        public YogiBearEventArgs(Boolean isWon, Int32 gameTime, Int32 gameTable)
        {
            _isWon = isWon;
            _gameTime = gameTime;
            _gameTable = gameTable;
        }
    }
}
