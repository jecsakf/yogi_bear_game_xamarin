using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using YogiBearGame.ViewModel;
using YogiBearGame.Model;

namespace YogiBearGame.ViewModel
{
    public class YogiBearViewModel : ViewModelBase
    {
        #region Fields

        private YogiBearGameModel _model;

        #endregion

        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand LoadGameTablesCommand { get; private set; }

        public DelegateCommand ExitCommand { get; private set; }

        public DelegateCommand StartStopCommand { get; private set; }

        public DelegateCommand KeyDownCommand { get; private set; }

        public ObservableCollection<YogiBearField> Fields { get; set; }

        public String GameTime { get { return TimeSpan.FromSeconds(_model.GameTime).ToString("g"); } }

        public int PickedBasketsCount { get { return _model.Table.PickedBasketsCount; } }

        public int GameTableSize { get { return _model.Table.Size; } }

        public bool IsGameTableLoaded { get { return _model.LoadedTables.Count != 0; }}

        public bool IsSmallTableLoaded { get { return _model.LoadedTables.ContainsKey("small"); } }

        public bool IsMediumTableLoaded { get { return _model.LoadedTables.ContainsKey("medium"); } }

        public bool IsLargeTableLoaded { get { return _model.LoadedTables.ContainsKey("large"); } }
        public Boolean IsGameTableSmall
        {
            get { return _model.GameTable == GameTable.Small; }
            set
            {
                if (_model.GameTable == GameTable.Small)
                    return;

                _model.GameTable = GameTable.Small;
                OnPropertyChanged("IsGameTableSmall");
                OnPropertyChanged("IsGameTableMedium");
                OnPropertyChanged("IsGameTableLarge");
            }
        }

        public Boolean IsGameTableMedium
        {
            get { return _model.GameTable == GameTable.Medium; }
            set
            {
                if (_model.GameTable == GameTable.Medium)
                    return;

                _model.GameTable = GameTable.Medium;
                OnPropertyChanged("IsGameTableSmall");
                OnPropertyChanged("IsGameTableMedium");
                OnPropertyChanged("IsGameTableLarge");
            }
        }

        public Boolean IsGameTableLarge
        {
            get { return _model.GameTable == GameTable.Large; }
            set
            {
                if (_model.GameTable == GameTable.Large)
                    return;

                _model.GameTable = GameTable.Large;
                OnPropertyChanged("IsGameTableSmall");
                OnPropertyChanged("IsGameTableMedium");
                OnPropertyChanged("IsGameTableLarge");
            }
        }

        public string IsGameNotStarted { get; set; }

        public int ViewHeight { get; set; }

        public int ViewWidth { get; set; }


        #endregion

        #region Events

        public event EventHandler NewGame;

        public event EventHandler LoadGameTables;

        public event EventHandler ExitGame;

        public event EventHandler StartStop;

        #endregion

        #region Constructors

        /// <summary>
        /// Sudoku nézetmodell példányosítása.
        /// </summary>
        /// <param name="model">A modell típusa.</param>
        public YogiBearViewModel(YogiBearGameModel model)
        {
            // játék csatlakoztatása
            _model = model;
            _model.GameAdvanced += new EventHandler<YogiBearEventArgs>(Model_GameAdvanced);
            _model.GameOver += new EventHandler<YogiBearEventArgs>(Model_GameOver);
            _model.GameCreated += new EventHandler<YogiBearEventArgs>(Model_GameCreated);
            _model.GameTableLoaded += new EventHandler<YogiBearEventArgs>(Model_GameTableLoaded);
            
            // parancsok kezelése
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameTablesCommand = new DelegateCommand(param => OnLoadGameTables());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            StartStopCommand = new DelegateCommand(param => OnStartStop());
            KeyDownCommand = new DelegateCommand(param => StepGame(Convert.ToString(param)));

            // játéktábla létrehozása
            Fields = new ObservableCollection<YogiBearField>();
            for (Int32 i = 0; i < _model.Table.Size; i++) // inicializáljuk a mezőket
            {
                for (Int32 j = 0; j < _model.Table.Size; j++)
                {
                    Fields.Add(new YogiBearField
                    {
                        Value = _model.Table[i, j],
                        Image = "/Resources/empty.png",
                        X = i,
                        Y = j,
                        Number = i * _model.Table.Size + j, // a gomb sorszáma, amelyet felhasználunk az azonosításhoz
                        StepCommand = new DelegateCommand(param => StepGame(Convert.ToString(param)))
                        // ha egy mezőre léptek, akkor jelezzük a léptetést, változtatjuk a lépésszámot
                    }) ;
                }
            }

            IsGameNotStarted = "Hidden";
            OnPropertyChanged("IsGameNotStarted");
        }

        #endregion

        #region Public and private methods

        /// <summary>
        /// Tábla frissítése.
        /// </summary>
        public void RefreshTable()
        {
            foreach (YogiBearField field in Fields) // inicializálni kell a mezőket is
            {
                field.Value = _model.Table[field.X, field.Y];
                string imgpath = "";
                if (_model.Table[field.X, field.Y] == 0) imgpath = "/Resources/drawable/empty.png";
                else if (_model.Table[field.X, field.Y] == 1) imgpath = "/Resources/drawable/yogi_bear.png";
                else if (_model.Table[field.X, field.Y] == 2) imgpath = "/Resources/drawable/park_ranger.png";
                else if (_model.Table[field.X, field.Y] == 3) imgpath = "/Resources/drawable/tree.png";
                else if (_model.Table[field.X, field.Y] == 4) imgpath = "/Resources/drawable/picnic_basket.png";
                field.Image = imgpath;
            }

            OnPropertyChanged("GameTime");
        }
        /// <summary>
        /// Játék léptetése eseménykiváltása.
        /// </summary>
        /// <param name="index">A lépett mező indexe.</param>
        private void StepGame(string key)
        {
            if (_model.GameIsOn)
            {
                switch (key) // megkapjuk a billentyűt
                {
                    case "A":
                        _model.Step(0, -1);
                        break;
                    case "S":
                        _model.Step(1, 0);
                        break;
                    case "D":
                        _model.Step(0, 1);
                        break;
                    case "W":
                        _model.Step(-1, 0);
                        break;
                }
                OnPropertyChanged("PickedBasketsCount"); // jelezzük a változást
                RefreshTable();
            }
        }

        #endregion

        #region Game event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void Model_GameOver(object sender, YogiBearEventArgs e)
        {
            OnPropertyChanged("PickedBasketsCount");
            RefreshTable();
        }

        /// <summary>
        /// Játék előrehaladásának eseménykezelője.
        /// </summary>
        private void Model_GameAdvanced(object sender, YogiBearEventArgs e)
        {
            OnPropertyChanged("GameTime");
            RefreshTable();
        }

        /// <summary>
        /// Játék létrehozásának eseménykezelője.
        /// </summary>
        private void Model_GameCreated(object sender, YogiBearEventArgs e)
        {
            Fields = new ObservableCollection<YogiBearField>();
            for (Int32 i = 0; i < _model.Table.Size; i++) // inicializáljuk a mezőket
            {
                for (Int32 j = 0; j < _model.Table.Size; j++)
                {
                    string imgpath = "";
                    if (_model.Table[i, j] == 0) imgpath = "/Resources/drawable/empty.png";
                    else if (_model.Table[i, j] == 1) imgpath = "/Resources/drawable/yogi_bear.png";
                    else if (_model.Table[i, j] == 2) imgpath = "/Resources/drawable/park_ranger.png";
                    else if (_model.Table[i, j] == 3) imgpath = "/Resources/drawable/tree.png";
                    else if (_model.Table[i, j] == 4) imgpath = "/Resources/drawable/picnic_basket.png";

                    Fields.Add(new YogiBearField
                    {
                        Value = _model.Table[i, j],
                        Image = imgpath,
                        X = i,
                        Y = j,
                        Number = i * _model.Table.Size + j, // a gomb sorszáma, amelyet felhasználunk az azonosításhoz
                        StepCommand = new DelegateCommand(param => StepGame(Convert.ToString(param)))
                        // ha egy mezőre léptek, akkor jelezzük a léptetést, változtatjuk a lépésszámot
                    });
                }
            }

            IsGameNotStarted = "Visible";
            OnPropertyChanged("PickedBasketsCount");
            OnPropertyChanged("IsGameNotStarted");
            OnPropertyChanged("GameTableSize");
            OnPropertyChanged("Fields");
            RefreshTable();
        }

        public void Model_GameTableLoaded(object sender, YogiBearEventArgs e)
        {
            if (e.GameTable == 1) OnPropertyChanged("IsSmallTableLoaded");
            else if (e.GameTable == 2) OnPropertyChanged("IsMediumTableLoaded");
            else if (e.GameTable == 3) OnPropertyChanged("IsLargeTableLoaded");

            if(e.GameTime != 0)
            {

            }
        }

        #endregion

        #region Event methods

        private void OnNewGame()
        {
            if (NewGame != null)
                NewGame(this, EventArgs.Empty);
        }

        private void OnLoadGameTables()
        {
            if (LoadGameTables != null)
                LoadGameTables(this, EventArgs.Empty);
        }

        private void OnExitGame()
        {
            if (ExitGame != null)
                ExitGame(this, EventArgs.Empty);
        }

        private void OnStartStop()
        {
            if (StartStop != null)
                StartStop(this, EventArgs.Empty);
        }

        #endregion
    }
}
