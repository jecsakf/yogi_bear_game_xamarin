using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace YogiBearGame.Persistence
{
    public class YogiBearTable
    {
        #region Fields

        private int _tableSize;
        private List<int> _baskets;
        private List<int> _trees;
        private int[,] _table;
        private List<int> _rangers;
        private (char, int, int)[] _rangersDirection;
        private int _yogiPosition;
        private List<int> _pickedBaskets;
        private bool _firstRunning;

        #endregion

        #region Properties

        public int Size { get { return _tableSize; } }

        public Int32 this[Int32 x, Int32 y] { get { return GetValue(x, y); } set { SetValue(x, y, value); } }

        public List<int> Baskets { get { return _baskets; } }

        public List<int> Rangers { get { return _rangers; } }

        public List<int> Trees { get { return _trees; } }

        public (char,int,int)[] RangersDirection { get { return _rangersDirection; } }
        public int BasketsCount { get { return _baskets.Count; } }
        public List<int> PickedBaskets { get { return _pickedBaskets; }}

        public int PickedBasketsCount { get { return _pickedBaskets.Count; } }

        public int YogiPosition { get { return _yogiPosition; } set { _yogiPosition = value; } }

        public bool FirstRunning { get { return _firstRunning; } set { _firstRunning = value; } }
        #endregion

        #region Constructors

        /// <summary>
        /// Sudoku játéktábla példányosítása.
        /// </summary>
        public YogiBearTable()
        {
            _tableSize = 6;
            _baskets = new List<int>() { 5, 6, 10, 14, 26, 30 };
            _trees = new List<int>() { 9, 13, 34, 27 };
            _table = new int[_tableSize, _tableSize];
            _rangers = new List<int>() { 18, 4 };
            _rangersDirection = new(char, int, int)[] { ('r', 18, 23), ('d', 4, 34) };
            _yogiPosition = 0;
            _pickedBaskets = new List<int>();
            FirstRunning = true;
        }

        /// <summary>
        /// Sudoku játéktábla példányosítása.
        /// </summary>
        /// <param name="tableSize">Játéktábla mérete.</param>
        /// <param name="regionSize">Ház mérete.</param>
        public YogiBearTable(int tableSize, List<int> baskets, List<int> rangers, List<int> trees, (char, int, int)[] rangersDirection)
        {
            if (tableSize < 0)
                throw new ArgumentOutOfRangeException("The table size is less than 0.", "tableSize");
            if (baskets.Count < 0)
                throw new ArgumentOutOfRangeException("The baskets count is less than 0.", "basketsNumber");
            if (rangers.Count < 0)
                throw new ArgumentOutOfRangeException("The rangers count is less than 0.", "rangersNumber");
            if (trees.Count < 0)
                throw new ArgumentOutOfRangeException("The trees count is less than 0.", "treesNumber");

            _tableSize = tableSize;
            _baskets = baskets;
            _rangers = rangers;
            _trees = trees;
            _table = new int[_tableSize, _tableSize];
            _rangers = rangers;
            _rangersDirection = rangersDirection;
            _yogiPosition = 0;
            _pickedBaskets = new List<int>();
            FirstRunning = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Mező értékének lekérdezése.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <returns>A mező értéke.</returns>
        public Int32 GetValue(int x, int y)
        {
            if (x < 0 || x >= _tableSize)
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= _tableSize)
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");

            return _table[x, y];
        }

        /// <summary>
        /// Mező értékének beállítása.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <param name="value">Érték.</param>
        /// <param name="lockField">Zárolja-e a mezőt.</param>
        public void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= _tableSize)
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= _tableSize)
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");
            if (value < 0 || value > 4)
                throw new ArgumentOutOfRangeException("value", "The value is out of range.");

            _table[x, y] = value;
            if (value == 1)
            {
                _yogiPosition = x * _tableSize + y;
            }
        }

        public void StepRangers()
        {
            for (int i = 0; i < _rangers.Count; i++)
            {
                int x = _rangers[i] / _tableSize;
                int y = _rangers[i] % _tableSize;
                if (_rangersDirection[i].Item1 == 'r')
                {
                    if (FirstRunning == true)
                    {
                        _rangersDirection[i].Item2 = _rangers[i];
                        _rangersDirection[i].Item3 = EndPoint('h', x, y);
                    }
                    if(y == _rangersDirection[i].Item3 % _tableSize)
                    {
                        ChangePoints(ref _rangersDirection[i].Item2, ref _rangersDirection[i].Item3);
                        _rangersDirection[i].Item1 = 'l';
                    }
                    if (_rangersDirection[i].Item1 == 'r') _rangers[i] = x * _tableSize + y + 1;
                    else if (_rangersDirection[i].Item1 == 'l') _rangers[i] = x * _tableSize + y - 1;
                }
                else if (_rangersDirection[i].Item1 == 'l')
                {
                    if (FirstRunning == true)
                    {
                        _rangersDirection[i].Item2 = _rangers[i];
                        _rangersDirection[i].Item3 = EndPoint('h', x, y);
                    }
                    if (y == _rangersDirection[i].Item3 % _tableSize)
                    {
                        ChangePoints(ref _rangersDirection[i].Item2, ref _rangersDirection[i].Item3);
                        _rangersDirection[i].Item1 = 'r';
                    }
                    if (_rangersDirection[i].Item1 == 'r') _rangers[i] = x * _tableSize + y + 1;
                    else if (_rangersDirection[i].Item1 == 'l') _rangers[i] = x * _tableSize + y - 1;
                }
                else if (_rangersDirection[i].Item1 == 'u')
                {
                    if (FirstRunning == true)
                    {
                        _rangersDirection[i].Item2 = _rangers[i];
                        _rangersDirection[i].Item3 = EndPoint('v', x, y);
                    }
                    if (x == _rangersDirection[i].Item3 / _tableSize)
                    {
                        ChangePoints(ref _rangersDirection[i].Item2, ref _rangersDirection[i].Item3);
                        _rangersDirection[i].Item1 = 'd';
                    }
                    if (_rangersDirection[i].Item1 == 'u') _rangers[i] = (x-1) * _tableSize + y;
                    else if (_rangersDirection[i].Item1 == 'd') _rangers[i] = (x+1) * _tableSize + y;
                }
                else if (_rangersDirection[i].Item1 == 'd')
                {
                    if (FirstRunning == true)
                    {
                        _rangersDirection[i].Item2 = _rangers[i];
                        _rangersDirection[i].Item3 = EndPoint('v', x, y);
                    }
                    if (x == _rangersDirection[i].Item3 / _tableSize)
                    {
                        ChangePoints(ref _rangersDirection[i].Item2, ref _rangersDirection[i].Item3);
                        _rangersDirection[i].Item1 = 'u';
                    }
                    if (_rangersDirection[i].Item1 == 'u') _rangers[i] = (x - 1) * _tableSize + y;
                    else if (_rangersDirection[i].Item1 == 'd') _rangers[i] = (x + 1) * _tableSize + y;
                }
            }
            FirstRunning = false;
        }

        public bool CheckStep(int _x, int _y)
        {

            if (_x < 0 || _x >= _tableSize)
                return false;
            if (_y < 0 || _y >= _tableSize)
                return false;
            if (_table[_x, _y] == 2 || _table[_x, _y] == 3)
                return false;

            if (_table[_x, _y] == 4)
            {
                _pickedBaskets.Add(_x * _tableSize + _y);
                _table[_x, _y] = 0;
            }
                

            return true;
        }

        public bool RangerSeeYogi()
        {
            for (int i = 0; i < _rangers.Count; i++)
            {
                int x = _rangers[i] / _tableSize;
                int y = _rangers[i] % _tableSize;
                ArrayList fieldInView = new ArrayList();
                for (int j = x - 1; j < x + 2; j++)
                {
                    for (int k = y - 1; k < y + 2; k++)
                    {
                        if (j < 0 || j > _tableSize-1 || k < 0 || k > _tableSize-1) continue;
                        fieldInView.Add(j * _tableSize + k);
                    }
                }
                    
                if (fieldInView.Contains(_yogiPosition))
                    return true;
            }
            return false;
        }

        #endregion

        #region Private methods

        public int EndPoint(char vOrH, int x, int y)
        {
            int endPoint = -1;
            if (vOrH == 'v')
            {
                if (x == 0) endPoint = _tableSize*(_tableSize-1)+y;
                else endPoint = y;
                List<int> lineFields = new List<int>();
                if (x == 0)
                {
                    for (int i = 0; i < _tableSize; i++)
                        lineFields.Add(i * _tableSize + y);
                }
                else if (x == (_tableSize * (_tableSize - 1) + y) / _tableSize)
                {
                    for (int i = _tableSize - 1; i >= 0; i--)
                        lineFields.Add(i * _tableSize + y);
                }
                List<int> intersection = lineFields.Intersect(_trees).ToList();
                intersection.Sort();
                if (intersection.Count != 0)
                    if (x == 0) endPoint = intersection[0]-_tableSize;
                    else endPoint = intersection[intersection.Count-1]+_tableSize;
            }
            else if (vOrH == 'h')
            {
                if (y == 0) endPoint = x*_tableSize + _tableSize - 1;
                else endPoint = x*_tableSize;
                List<int> lineFields = new List<int>();
                if (y == 0)
                {
                    for (int i = 0; i < _tableSize; i++)
                        lineFields.Add(x * _tableSize + i);
                }
                else if (y == (x * _tableSize + _tableSize - 1) % _tableSize)
                {
                    for (int i = _tableSize-1; i >= 0; i--)
                        lineFields.Add(x * _tableSize + i);
                }
                List<int> intersection = lineFields.Intersect(_trees).ToList();
                intersection.Sort();
                if (intersection.Count != 0)
                    if (y == 0) endPoint = intersection[0]-1;
                    else endPoint = intersection[intersection.Count - 1]+1;
            }
            return endPoint;
        }

        private void ChangePoints(ref int s, ref int e)
        {
            int tmp = s;
            s = e;
            e = tmp;
        }

        #endregion

    }
}
