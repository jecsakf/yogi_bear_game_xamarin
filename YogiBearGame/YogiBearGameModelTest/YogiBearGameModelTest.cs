using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using YogiBearGame.Model;
using YogiBearGame.Persistence;

namespace YogiBearGameModelTest
{
    [TestClass]
    public class YogiBearGameModelTest
    {
        private YogiBearGameModel _model;
        private YogiBearTable _mockedTable;
        private Mock<IYogiBearDataAccess> _mock;

        [TestInitialize]
        public void Initialize()
        {
            _mockedTable = new YogiBearTable();
            for (int i = 0; i < _mockedTable.Size; i++)
                for (int j = 0; j < _mockedTable.Size; j++)
                    _mockedTable[i, j] = 0;
            _mockedTable[0, 0] = 1;
            _mockedTable[0, 5] = 4;
            _mockedTable[1, 0] = 4;
            _mockedTable[1, 3] = 3;
            _mockedTable[1, 4] = 4;
            _mockedTable[2, 1] = 3;
            _mockedTable[2, 2] = 4;
            _mockedTable[3, 0] = 2;
            _mockedTable[4, 0] = 3;
            _mockedTable[4, 2] = 4;
            _mockedTable[4, 3] = 3;
            _mockedTable[5, 0] = 4;
            _mockedTable[5, 4] = 2;
            // el?re defini�lunk egy j�t�kt�bl�t a perzisztencia mockolt tesztel�s�hez

            _mock = new Mock<IYogiBearDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult((_mockedTable,0)));
            // a mock a LoadAsync m?veletben b�rmilyen param�terre az el?re be�ll�tott j�t�kt�bl�t fogja visszaadni

            _model = new YogiBearGameModel(_mock.Object);
            // p�ld�nyos�tjuk a modellt a mock objektummal

            _model.GameAdvanced += new EventHandler<YogiBearEventArgs>(Model_GameAdvanced);
            _model.GameOver += new EventHandler<YogiBearEventArgs>(Model_GameOver);

            _model.LoadedTables.Add("small", _mockedTable);
            _model.LoadedTables.Add("medium", _mockedTable);
            _model.LoadedTables.Add("large", _mockedTable);
        }

        [TestMethod]
        public void YogiBearGameModelNewGameSmallTest()
        {
            _model.NewGame();

            Assert.AreEqual(GameTable.Small, _model.GameTable); // a p�lya be�ll�t�dott
            Assert.AreEqual(0, _model.GameTime); // 0-r�l indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?poz�ci�n �ll (bal sarok)

            Int32 emptyFields = 0;
            for (Int32 i = 0; i < 6; i++)
                for (Int32 j = 0; j < 6; j++)
                    if (_model.Table[i, j] == 0)
                        emptyFields++;

            Assert.AreEqual(23, emptyFields); // szabad mez?k sz�ma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelNewGameMediumTest()
        {
            _model.GameTable = GameTable.Medium;
            _model.NewGame();

            Assert.AreEqual(GameTable.Medium, _model.GameTable); // a p�lya be�ll�t�dott
            Assert.AreEqual(0, _model.GameTime); // 0-r�l indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?poz�ci�n �ll (bal sarok)

            Int32 emptyFields = 12 * 12 - 1 - 4 - 8 - 12;
            Assert.AreEqual(119, emptyFields); // szabad mez?k sz�ma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelNewGameLargeTest()
        {
            _model.GameTable = GameTable.Large;
            _model.NewGame();

            Assert.AreEqual(GameTable.Large, _model.GameTable); // a p�lya be�ll�t�dott
            Assert.AreEqual(0, _model.GameTime); // 0-r�l indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?poz�ci�n �ll (bal sarok)

            Int32 emptyFields = 18 * 18 - 1 - 6 - 12 - 18;

            Assert.AreEqual(287, emptyFields); // szabad mez?k sz�ma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelStepTest()
        {
            Assert.AreEqual(0, _model.Table.YogiPosition); // m�g nem l�pt�nk

            _model.Step(0, 1);

            Assert.AreEqual(0, _model.Table.YogiPosition); // mivel a j�t�k �ll, nem szabad, hogy l�pj�nk

            _model.NewGame();

            _model.Step(0, 1);

            Assert.AreEqual(1, _model.Table.YogiPosition); // most m�r l�pt�nk
            Assert.AreEqual(0, _model.Table[0, 0]);

            _model.Step(1, 0);

            Assert.AreEqual(7, _model.Table.YogiPosition); // most m�r l�pt�nk
            Assert.AreEqual(0, _model.Table[0, 1]);

            _model.Step(0, -1);

            Assert.AreEqual(6, _model.Table.YogiPosition); // most m�r l�pt�nk
            Assert.AreEqual(0, _model.Table[1, 1]);

            _model.Step(0, -1);
            Assert.AreEqual(6, _model.Table.YogiPosition); // maradtunk, ahol voltunk, mert ki akartunk l�pni a p�ly�r�l

            _model.Step(-1, 0);
            _model.Step(-1, 0);
            Assert.AreEqual(0, _model.Table.YogiPosition); // az els? l�p�s �rv�nyes, a k�vetkez?vel megint kil�pt�nk volna a p�ly�r�l

            _model.Step(1, 0);
            _model.Step(1, 0);
            _model.Step(0, 1);
            Assert.AreEqual(12, _model.Table.YogiPosition); // nem tudtunk jobbra l�pni, mert ott fa van
        }

        [TestMethod]
        public void YogiBearGameModelAdvanceTimeTest()
        {
            _model.NewGame();

            Int32 time = _model.GameTime;

            _model.AdvanceTime();

            time++;

            Assert.AreEqual(time, _model.GameTime); // az id? n?tt
            Assert.AreEqual(19, _model.Table.Rangers[0]); // �s l�ptek az ?r�k
            Assert.AreEqual(10, _model.Table.Rangers[1]);

            _model.GameIsOn = false;
            _model.AdvanceTime();
            _model.AdvanceTime();

            Assert.AreEqual(time, _model.GameTime); // az id? nem v�ltozott, mert �ll a j�t�k
            Assert.AreEqual(19, _model.Table.Rangers[0]); // nem l�ptek az ?r�k sem
            Assert.AreEqual(10, _model.Table.Rangers[1]);

            _model.GameIsOn = true;
            _model.AdvanceTime();
            time++;
            Assert.AreEqual(time, _model.GameTime); // az id? v�ltozott, �jra megy a j�t�k
            Assert.AreEqual(20, _model.Table.Rangers[0]); // l�ptek az ?r�k is
            Assert.AreEqual(16, _model.Table.Rangers[1]);
        }

        [TestMethod]
        public async Task YogiBearGameModelLoadGameAsync()
        {
            _model.NewGame();

            await _model.LoadGameAsync(String.Empty);

            // az id? 0-ra �ll vissza
            Assert.AreEqual(0, _model.GameTime);

            // ellen?rizz�k, hogy megh�vt�k-e a Load m?veletet a megadott param�terrel
            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }

        private void Model_GameAdvanced(Object sender, YogiBearEventArgs e)
        {
            Assert.IsTrue(_model.GameTime >= 0); // a j�t�kid? nem lehet negat�v
            Assert.AreEqual(_model.Table.BasketsCount == _model.Table.PickedBasketsCount || _model.Table.RangerSeeYogi(), _model.IsGameOver); // a tesztben a j�t�knak csak akkor lehet v�ge, ha felvett�nk minden kosarat, vagy megl�tott egy ?r

            Assert.AreEqual(e.GameTime, _model.GameTime); // a k�t �rt�knek egyeznie kell
            Assert.IsFalse(e.IsWon); // m�g nem nyert�k meg a j�t�kot
        }

        private void Model_GameOver(Object sender, YogiBearEventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver); // biztosan v�ge van a j�t�knak
            Assert.AreEqual(true, _model.Table.BasketsCount == _model.Table.PickedBasketsCount || _model.Table.RangerSeeYogi()); // a tesztben csak akkor v�lt�dhat ki, ha felvett�nk minden kosarat, vagy megl�tott egy ?r
            if (_model.Table.BasketsCount == _model.Table.PickedBasketsCount) Assert.IsTrue(e.IsWon);
            if (_model.Table.RangerSeeYogi()) Assert.IsFalse(e.IsWon);
        }
    }
}
