using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YogiBearGame.Persistence;

namespace YogiBearGame.Model
{
    /// <summary>
    /// Játéktáblaméretének felsorolási típusa.
    /// </summary>
    public enum GameTable { Small, Medium, Large }
    public class YogiBearGameModel
    {
        #region Fields
        
        private IYogiBearDataAccess _dataAccess; // adatelérés
        private YogiBearTable _table; // játéktábla
        private Dictionary<string,YogiBearTable> _loadedTables= new Dictionary<string, YogiBearTable>();
        private GameTable _gameTable; // pálya
        private Int32 _gameTime; // játékidő
        private bool _gameIsOn;

        #endregion

        #region Properties

        /// <summary>
        /// Betöltött pályák lekérdezése.
        /// </summary>
        public Dictionary<string, YogiBearTable> LoadedTables { get { return _loadedTables; } }

        public bool GameIsOn { get { return _gameIsOn; } set { _gameIsOn = value; } }

        /// <summary>
        /// Hátramaradt játékidő lekérdezése.
        /// </summary>
        public Int32 GameTime { get { return _gameTime; } }

        /// <summary>
        /// Játéktábla lekérdezése.
        /// </summary>
        public YogiBearTable Table { get { return _table; } }

        /// <summary>
        /// Játék végének lekérdezése.
        /// </summary>
        public Boolean IsGameOver { get { return _table.PickedBasketsCount == _table.BasketsCount || _table.RangerSeeYogi(); } }

        /// <summary>
        /// Játékpálya lekérdezése, vagy beállítása.
        /// </summary>
        public GameTable GameTable { get { return _gameTable; } set { _gameTable = value; } }

        #endregion

        #region Events

        /// <summary>
        /// Játék előrehaladásának eseménye.
        /// </summary>
        public event EventHandler<YogiBearEventArgs> GameAdvanced;

        /// <summary>
        /// Játék végének eseménye.
        /// </summary>
        public event EventHandler<YogiBearEventArgs> GameOver;

        public event EventHandler<YogiBearEventArgs> GameCreated;

        public event EventHandler<YogiBearEventArgs> GameTableLoaded;
        #endregion

        #region Constructor

        /// <summary>
        /// Sudoku játék példányosítása.
        /// </summary>
        /// <param name="dataAccess">Az adatelérés.</param>
        public YogiBearGameModel(IYogiBearDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _table = new YogiBearTable();
            _gameTable = GameTable.Small;
            GenerateFields();
        }

        #endregion

        #region Public methods

        public void NewGame()
        {
            _gameTime = 0;
            _gameIsOn = true;

            switch (_gameTable)
            {
                case GameTable.Small:
                    YogiBearTable _newTable = _loadedTables["small"];
                    List<int> r = new List<int>(_newTable.Rangers);
                    (char, int, int)[] rd = new (char, int, int)[_newTable.RangersDirection.Length];
                    _newTable.RangersDirection.CopyTo(rd,0);
                    _table = new YogiBearTable(_newTable.Size, _newTable.Baskets, r, _newTable.Trees, rd);
                    GenerateFields();
                    break;
                case GameTable.Medium:
                    YogiBearTable _newTableM = _loadedTables["medium"];
                    List<int> rM = new List<int>(_newTableM.Rangers);
                    (char, int, int)[] rdM = new (char, int, int)[_newTableM.RangersDirection.Length];
                    _newTableM.RangersDirection.CopyTo(rdM, 0);
                    _table = new YogiBearTable(_newTableM.Size, _newTableM.Baskets, rM, _newTableM.Trees, rdM);
                    GenerateFields();
                    break;
                case GameTable.Large:
                    YogiBearTable _newTableL = _loadedTables["large"];
                    List<int> rL = new List<int>(_newTableL.Rangers);
                    (char, int, int)[] rdL = new (char, int, int)[_newTableL.RangersDirection.Length];
                    _newTableL.RangersDirection.CopyTo(rdL, 0);
                    _table = new YogiBearTable(_newTableL.Size, _newTableL.Baskets, rL, _newTableL.Trees, rdL);
                    GenerateFields();
                    break;
            }
            OnGameCreated();
        }

        /// <summary>
        /// Játékidő léptetése.
        /// </summary>
        public void AdvanceTime()
        {
            if (IsGameOver) // ha már vége, nem folytathatjuk
                return;
            if (!_gameIsOn) return;
            _gameTime++;
            int [] _positions = new int[_table.Rangers.Count];
            _table.Rangers.CopyTo(_positions);
            _table.StepRangers();
            for (int i = 0; i < _table.Rangers.Count; i++)
            {
                if (_table.Baskets.Contains(_positions[i]) && !_table.PickedBaskets.Contains(_positions[i])) _table[_positions[i] / _table.Size, _positions[i] % _table.Size] = 4;
                else _table[_positions[i] / _table.Size, _positions[i] % _table.Size] = 0;
                _table[_table.Rangers[i] / _table.Size, _table.Rangers[i] % _table.Size] = 2;
            }
            if (_table.RangerSeeYogi())
                OnGameOver(false);
            OnGameAdvanced();
        }

        /// <summary>
        /// Táblabeli lépés végrehajtása.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        public void Step(Int32 mx, Int32 my)
        {
            if (IsGameOver) return;  // ha már vége a játéknak, nem játszhatunk 
            if (!_gameIsOn) return;

            int x = _table.YogiPosition / _table.Size;
            int y = _table.YogiPosition % _table.Size;
            if (_table.CheckStep(x+mx, y+my))
            {
                _table.SetValue(x, y, 0);
                _table.SetValue(x + mx, y + my, 1);
                _table.YogiPosition = (x + mx) * _table.Size + (y + my);
            }

            if (_table.PickedBasketsCount == _table.BasketsCount)
                OnGameOver(true);
            if (_table.RangerSeeYogi())
                OnGameOver(false);
            
            OnGameAdvanced();
        }

        /// <summary>
        /// Játékpálya betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        public async Task LoadGameAsync(string path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            (YogiBearTable, int) pair = await _dataAccess.LoadAsync(path);
            _table = pair.Item1;
            _gameTime = pair.Item2;
            if (path != String.Empty && !_loadedTables.ContainsKey(Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length - 4)) && _gameTime == 0)
                _loadedTables.Add(Path.GetFileName(path).Substring(0, Path.GetFileName(path).Length-4), _table);

            if(_gameTime != 0)
                _gameIsOn = true;

            OnGameTableLoaded();
        }

        /// <summary>
        /// Játékpálya mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        public async Task SaveGameAsync(string path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await _dataAccess.SaveAsync(path, _table, _gameTime);
        }

        #endregion

        #region Private game methods

        /// <summary>
        /// Mezők generálása.
        /// </summary>
        /// <param name="count">Mezők száma.</param>
        private void GenerateFields()
        {
            for (Int32 i = 0; i < Table.Size; i++)
            {
                for (Int32 j = 0; j < Table.Size; j++)
                {
                    if (i == 0 && j == 0) Table.SetValue(i, j, 1);
                    else if (Table.Baskets.Contains(i * Table.Size + j)) Table.SetValue(i, j, 4);
                    else if (Table.Rangers.Contains(i * Table.Size + j)) Table.SetValue(i, j, 2);
                    else if (Table.Trees.Contains(i * Table.Size + j)) Table.SetValue(i, j, 3);
                    else Table.SetValue(i, j, 0);
                }
            }
        }

        #endregion

        #region Private event methods

        /// <summary>
        /// Játékidő változás eseményének kiváltása.
        /// </summary>
        private void OnGameAdvanced()
        {
            if (GameAdvanced != null)
                GameAdvanced(this, new YogiBearEventArgs(false, _gameTime, 0));
        }

        /// <summary>
        /// Játék vége eseményének kiváltása.
        /// </summary>
        /// <param name="isWon">Győztünk-e a játékban.</param>
        private void OnGameOver(Boolean isWon)
        {
            if (GameOver != null)
                GameOver(this, new YogiBearEventArgs(isWon,_gameTime,0));
        }

        private void OnGameCreated()
        {
            if (GameCreated != null)
                GameCreated(this, new YogiBearEventArgs(false, _gameTime,0));
        }

        private void OnGameTableLoaded()
        {
            int table = 0;
            if (_loadedTables.ContainsKey("small")) table = 1;
            else if (_loadedTables.ContainsKey("medium")) table = 2;
            else if(_loadedTables.ContainsKey("large")) table = 3;
            if (GameTableLoaded != null)
                GameTableLoaded(this, new YogiBearEventArgs(false, _gameTime, table));
        }
        #endregion

    }
}
