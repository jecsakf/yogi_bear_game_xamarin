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
            // el?re definiálunk egy játéktáblát a perzisztencia mockolt teszteléséhez

            _mock = new Mock<IYogiBearDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult((_mockedTable,0)));
            // a mock a LoadAsync m?veletben bármilyen paraméterre az el?re beállított játéktáblát fogja visszaadni

            _model = new YogiBearGameModel(_mock.Object);
            // példányosítjuk a modellt a mock objektummal

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

            Assert.AreEqual(GameTable.Small, _model.GameTable); // a pálya beállítódott
            Assert.AreEqual(0, _model.GameTime); // 0-ról indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?pozíción áll (bal sarok)

            Int32 emptyFields = 0;
            for (Int32 i = 0; i < 6; i++)
                for (Int32 j = 0; j < 6; j++)
                    if (_model.Table[i, j] == 0)
                        emptyFields++;

            Assert.AreEqual(23, emptyFields); // szabad mez?k száma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelNewGameMediumTest()
        {
            _model.GameTable = GameTable.Medium;
            _model.NewGame();

            Assert.AreEqual(GameTable.Medium, _model.GameTable); // a pálya beállítódott
            Assert.AreEqual(0, _model.GameTime); // 0-ról indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?pozíción áll (bal sarok)

            Int32 emptyFields = 12 * 12 - 1 - 4 - 8 - 12;
            Assert.AreEqual(119, emptyFields); // szabad mez?k száma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelNewGameLargeTest()
        {
            _model.GameTable = GameTable.Large;
            _model.NewGame();

            Assert.AreEqual(GameTable.Large, _model.GameTable); // a pálya beállítódott
            Assert.AreEqual(0, _model.GameTime); // 0-ról indul a stopper
            Assert.AreEqual(0, _model.Table.YogiPosition); // Yogi a kezd?pozíción áll (bal sarok)

            Int32 emptyFields = 18 * 18 - 1 - 6 - 12 - 18;

            Assert.AreEqual(287, emptyFields); // szabad mez?k száma is megfelel?
        }

        [TestMethod]
        public void YogiBearGameModelStepTest()
        {
            Assert.AreEqual(0, _model.Table.YogiPosition); // még nem léptünk

            _model.Step(0, 1);

            Assert.AreEqual(0, _model.Table.YogiPosition); // mivel a játék áll, nem szabad, hogy lépjünk

            _model.NewGame();

            _model.Step(0, 1);

            Assert.AreEqual(1, _model.Table.YogiPosition); // most már léptünk
            Assert.AreEqual(0, _model.Table[0, 0]);

            _model.Step(1, 0);

            Assert.AreEqual(7, _model.Table.YogiPosition); // most már léptünk
            Assert.AreEqual(0, _model.Table[0, 1]);

            _model.Step(0, -1);

            Assert.AreEqual(6, _model.Table.YogiPosition); // most már léptünk
            Assert.AreEqual(0, _model.Table[1, 1]);

            _model.Step(0, -1);
            Assert.AreEqual(6, _model.Table.YogiPosition); // maradtunk, ahol voltunk, mert ki akartunk lépni a pályáról

            _model.Step(-1, 0);
            _model.Step(-1, 0);
            Assert.AreEqual(0, _model.Table.YogiPosition); // az els? lépés érvényes, a következ?vel megint kiléptünk volna a pályáról

            _model.Step(1, 0);
            _model.Step(1, 0);
            _model.Step(0, 1);
            Assert.AreEqual(12, _model.Table.YogiPosition); // nem tudtunk jobbra lépni, mert ott fa van
        }

        [TestMethod]
        public void YogiBearGameModelAdvanceTimeTest()
        {
            _model.NewGame();

            Int32 time = _model.GameTime;

            _model.AdvanceTime();

            time++;

            Assert.AreEqual(time, _model.GameTime); // az id? n?tt
            Assert.AreEqual(19, _model.Table.Rangers[0]); // és léptek az ?rök
            Assert.AreEqual(10, _model.Table.Rangers[1]);

            _model.GameIsOn = false;
            _model.AdvanceTime();
            _model.AdvanceTime();

            Assert.AreEqual(time, _model.GameTime); // az id? nem változott, mert áll a játék
            Assert.AreEqual(19, _model.Table.Rangers[0]); // nem léptek az ?rök sem
            Assert.AreEqual(10, _model.Table.Rangers[1]);

            _model.GameIsOn = true;
            _model.AdvanceTime();
            time++;
            Assert.AreEqual(time, _model.GameTime); // az id? változott, újra megy a játék
            Assert.AreEqual(20, _model.Table.Rangers[0]); // léptek az ?rök is
            Assert.AreEqual(16, _model.Table.Rangers[1]);
        }

        [TestMethod]
        public async Task YogiBearGameModelLoadGameAsync()
        {
            _model.NewGame();

            await _model.LoadGameAsync(String.Empty);

            // az id? 0-ra áll vissza
            Assert.AreEqual(0, _model.GameTime);

            // ellen?rizzük, hogy meghívták-e a Load m?veletet a megadott paraméterrel
            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }

        private void Model_GameAdvanced(Object sender, YogiBearEventArgs e)
        {
            Assert.IsTrue(_model.GameTime >= 0); // a játékid? nem lehet negatív
            Assert.AreEqual(_model.Table.BasketsCount == _model.Table.PickedBasketsCount || _model.Table.RangerSeeYogi(), _model.IsGameOver); // a tesztben a játéknak csak akkor lehet vége, ha felvettünk minden kosarat, vagy meglátott egy ?r

            Assert.AreEqual(e.GameTime, _model.GameTime); // a két értéknek egyeznie kell
            Assert.IsFalse(e.IsWon); // még nem nyerték meg a játékot
        }

        private void Model_GameOver(Object sender, YogiBearEventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver); // biztosan vége van a játéknak
            Assert.AreEqual(true, _model.Table.BasketsCount == _model.Table.PickedBasketsCount || _model.Table.RangerSeeYogi()); // a tesztben csak akkor váltódhat ki, ha felvettünk minden kosarat, vagy meglátott egy ?r
            if (_model.Table.BasketsCount == _model.Table.PickedBasketsCount) Assert.IsTrue(e.IsWon);
            if (_model.Table.RangerSeeYogi()) Assert.IsFalse(e.IsWon);
        }
    }
}
