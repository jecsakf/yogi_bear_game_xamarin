using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using YogiBearGame.Persistence;
using YogiBearGame.Droid.Persistence;
using System.Diagnostics;
using Android.Content.Res;

[assembly: Dependency(typeof(AndroidDataAccess))]
namespace YogiBearGame.Droid.Persistence
{
    public class AndroidDataAccess : IYogiBearDataAccess
    {
        private bool isTablesLoaded;
        int loadedTables;
        /// <summary>
        /// Fájl betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A beolvasott mezõértékek.</returns>

        public AndroidDataAccess()
        {
            isTablesLoaded = true;
            loadedTables = 0;
        }

        public async Task<(YogiBearTable,int)> LoadAsync(string filename)
        {
            string filePath = "";
            string content = "";
            if (isTablesLoaded)
            {
                AssetManager assets = Android.App.Application.Context.Assets;
                using (StreamReader reader = new StreamReader(assets.Open(filename)))
                {
                    content = reader.ReadToEnd();
                }
                loadedTables++;
                if(loadedTables == 3)
                    isTablesLoaded = false;
            }
            else {
                // a betöltés a személyen könyvtárból történik
                filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), filename);
                content = await Task.Run(() => File.ReadAllText(filePath));
            }

            // a fájlmûveletet taszk segítségével végezzük (aszinkron módon)
            String[] pieces = content.Split(' ');

            int tableSize = int.Parse(pieces[0]); // beolvassuk a tábla méretét

            List<int> baskets = new List<int>();
            baskets.AddRange(Array.ConvertAll(pieces[1].Split(','), s => int.Parse(s)));

            List<int> trees = new List<int>();
            trees.AddRange(Array.ConvertAll(pieces[2].Split(','), s => int.Parse(s)));

            List<int> rangers = new List<int>();
            rangers.AddRange(Array.ConvertAll(pieces[3].Split(','), s => int.Parse(s)));

            (char, int, int)[] rangersDirection = new (char, int, int)[rangers.Count];
            string[] directions = pieces[4].Split(',');
            for (int i = 0; i < directions.Length; i++)
            {
                rangersDirection[i] = (char.Parse(directions[i]), rangers[i], 0);
            }

            YogiBearTable table = new YogiBearTable(tableSize, baskets, rangers, trees, rangersDirection); // létrehozzuk a táblát

            for (Int32 i = 0; i < tableSize; i++)
            {
                for (Int32 j = 0; j < tableSize; j++)
                {
                    if (i == 0 && j == 0) table.SetValue(i, j, 1);
                    else if (baskets.Contains(i * tableSize + j)) table.SetValue(i, j, 4);
                    else if (rangers.Contains(i * tableSize + j)) table.SetValue(i, j, 2);
                    else if (trees.Contains(i * tableSize + j)) table.SetValue(i, j, 3);
                    else table.SetValue(i, j, 0);
                }
            }

            int gametime = 0;
            if (pieces.Length > 5)
            {
                for (int k = 0; k < rangers.Count; k++)
                {
                    string[] tmp = pieces[6].Split(',');
                    for (int m = 0; m < tmp.Length; m++)
                    {
                        string[] sePoints = tmp[m].Split('_'); 
                        rangersDirection[k] = (char.Parse(directions[k]), int.Parse(sePoints[0]), int.Parse(sePoints[1]));
                    }
                }

                table.PickedBaskets.AddRange(Array.ConvertAll(pieces[5].Split(','), s => int.Parse(s)));
                gametime = int.Parse(pieces[7]);
                int i = int.Parse(pieces[8]) / tableSize; 
                int j = int.Parse(pieces[8]) % tableSize;
                table.SetValue(i, j, 1);
                table.FirstRunning = false;
            }
            return (table, gametime);
        }

        /// <summary>
        /// Fájl mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <param name="table">A fájlba kiírandó játéktábla.</param>
        public async Task SaveAsync(String path, YogiBearTable table, int time)
        {
            string directions = "";
            string startEndPoints = "";
            for (int i = 0; i < table.RangersDirection.Length-1; i++)
            {
                directions += table.RangersDirection[i].Item1 + ",";
                startEndPoints += table.RangersDirection[i].Item2 + "_" + table.RangersDirection[i].Item3 + ",";
            }
            directions += table.RangersDirection[table.RangersDirection.Length - 1].Item1;
            startEndPoints += table.RangersDirection[table.RangersDirection.Length - 1].Item2 + "_" + table.RangersDirection[table.RangersDirection.Length - 1].Item3;


            String text = table.Size.ToString() + " "
                        + String.Join(",", table.Baskets.ToArray())+ " "
                        + String.Join(",", table.Trees.ToArray()) + " "
                        + String.Join(",", table.Rangers.ToArray()) + " "
                        + directions + " "
                        + startEndPoints + " "
                        + String.Join(",", table.PickedBaskets.ToArray()) + " "
                        + time.ToString() + " " + table.YogiPosition;
            

            // fájl létrehozása
            String filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), path);

            // kiírás (aszinkron módon)
            await Task.Run(() => File.WriteAllText(filePath, text));
        }
    }
}