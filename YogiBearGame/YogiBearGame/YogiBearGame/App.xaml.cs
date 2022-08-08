using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using YogiBearGame.Model;
using YogiBearGame.Persistence;
using YogiBearGame.ViewModel;
using YogiBearGame.View;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace YogiBearGame
{
    public partial class App : Application
    {
        private IYogiBearDataAccess _yogiBearDataAccess;
        private YogiBearGameModel _yogiBearGameModel;
        private YogiBearViewModel _yogiBearViewModel;
        private GamePage _gamePage;
        private SettingsPage _settingsPage;

        private IStore _store;
        private StoredGameBrowserModel _storedGameBrowserModel;
        private StoredGameBrowserViewModel _storedGameBrowserViewModel;

        private bool _advanceTimer;
        private NavigationPage _mainPage;

        public App()
        {
            //InitializeComponent();
            _yogiBearDataAccess = DependencyService.Get<IYogiBearDataAccess>();

            _yogiBearGameModel = new YogiBearGameModel(_yogiBearDataAccess);
            _yogiBearGameModel.GameOver += new EventHandler<YogiBearEventArgs>(YogiBearGameModel_GameOver);

            _yogiBearViewModel = new YogiBearViewModel(_yogiBearGameModel);
            _yogiBearViewModel.NewGame += new EventHandler(YogiBearViewModel_NewGame);
            _yogiBearViewModel.ExitGame += new EventHandler(YogiBearViewModel_ExitGame);
            _yogiBearViewModel.StartStop += new EventHandler(YogiBearViewModel_StartStop);

            _gamePage = new GamePage();
            _gamePage.BindingContext = _yogiBearViewModel;

            _settingsPage = new SettingsPage();
            _settingsPage.BindingContext = _yogiBearViewModel;

            _store = DependencyService.Get<IStore>(); // a perzisztencia betöltése az adott platformon
            _storedGameBrowserModel = new StoredGameBrowserModel(_store);
            _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
            _storedGameBrowserViewModel.GameLoading += new EventHandler<StoredGameEventArgs>(StoredGameBrowserViewModel_GameLoading);
            _storedGameBrowserViewModel.GameSaving += new EventHandler<StoredGameEventArgs>(StoredGameBrowserViewModel_GameSaving);

            _mainPage = new NavigationPage(_gamePage);

            MainPage = _mainPage;
        }

        protected override void OnStart()
        {
            GameTablesLoading();
            _yogiBearGameModel.NewGame();
            _yogiBearViewModel.RefreshTable();
            _advanceTimer = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { _yogiBearGameModel.AdvanceTime(); return _advanceTimer; }); // elindítjuk az időzítőt
        }

        protected override void OnSleep()
        {
            _advanceTimer = false;

            // elmentjük a jelenleg folyó játékot
            try
            {
                Task.Run(async () => await _yogiBearGameModel.SaveGameAsync("SuspendedGame"));
            }
            catch { }
        }

        protected override void OnResume()
        {
            // betöltjük a felfüggesztett játékot, amennyiben van
            try
            {
                Task.Run(async () =>
                {
                    await _yogiBearGameModel.LoadGameAsync("SuspendedGame");
                    _yogiBearViewModel.RefreshTable();

                    // csak akkor indul az időzítő, ha sikerült betölteni a játékot
                    _advanceTimer = true;
                    Device.StartTimer(TimeSpan.FromSeconds(1), () => { _yogiBearGameModel.AdvanceTime(); return _advanceTimer; });
                });
            }
            catch { }
        }

        public async void GameTablesLoading()
        {
            List<string> tablePaths = new List<string> { "small.txt", "medium.txt", "large.txt" };
            foreach (string path in tablePaths)
            {
                try
                {
                    await _yogiBearGameModel.LoadGameAsync(path);
                }
                catch
                {
                    await MainPage.DisplayAlert("Yogi Bear Game", "Gametables loading failed.", "OK");
                }
            }
        }

        public void YogiBearViewModel_NewGame(object sender, EventArgs e)
        {
            _yogiBearGameModel.NewGame();

            if (!_advanceTimer)
            {
                // ha nem fut az időzítő, akkor elindítjuk
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _yogiBearGameModel.AdvanceTime(); return _advanceTimer; });
            }
        }

        public void YogiBearViewModel_StartStop(object sender, EventArgs e)
        {
            if (_advanceTimer)
            {
                _yogiBearGameModel.GameIsOn = false;
                _advanceTimer = false;
            }
            else
            {
                _yogiBearGameModel.GameIsOn = true;
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _yogiBearGameModel.AdvanceTime(); return _advanceTimer; });
            }
        }

        public async void YogiBearViewModel_ExitGame(object sender, EventArgs e)
        {
            await _mainPage.PushAsync(_settingsPage);
        }

        public async void StoredGameBrowserViewModel_GameLoading(object sender, StoredGameEventArgs e)
        {
            try
            {
                await _yogiBearGameModel.LoadGameAsync(e.Name);
                _yogiBearViewModel.RefreshTable();

                // csak akkor indul az időzítő, ha sikerült betölteni a játékot
                _advanceTimer = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () => { _yogiBearGameModel.AdvanceTime(); return _advanceTimer; });
            }
            catch
            {
                await MainPage.DisplayAlert("Yogi Bear Game", "Game loading failed.", "OK");
            }
        }

        public async void StoredGameBrowserViewModel_GameSaving(object sender, StoredGameEventArgs e)
        {
            _advanceTimer = false;

            try
            {
                // elmentjük a játékot
                await _yogiBearGameModel.SaveGameAsync(e.Name);
            }
            catch { }

            await MainPage.DisplayAlert("Yogi Bear Game", "Success game saving.", "OK");
        }

        private async void YogiBearGameModel_GameOver(object sender, YogiBearEventArgs e)
        {
            _advanceTimer = false;
            if (e.IsWon) // győzelemtől függő üzenet megjelenítése
            {
                await MainPage.DisplayAlert("Congratulation, you win!" + Environment.NewLine +
                                "Game time: " + TimeSpan.FromSeconds(e.GameTime).ToString("g"),
                                "Yogi Bear Game",
                                "OK");
            }
            else
            {
                await MainPage.DisplayAlert("A ranger saw you." +
                                "Sorry, you lose!",
                                "Yogi Bear Game",
                                "OK");
            }
        }
    }
}
