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
using YogiBearGame.Persistence;
using Xamarin.Forms;
using YogiBearGame.Droid.Persistence;

[assembly: Dependency(typeof(AndroidStore))]
namespace YogiBearGame.Droid.Persistence
{
    public class AndroidStore : IStore
    {
        /// <summary>
        /// Fájlok lekérdezése.
        /// </summary>
        /// <returns>A fájlok listája.</returns>
        public async Task<IEnumerable<String>> GetFiles()
        {
            return await Task.Run(() => Directory.GetFiles(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)).Select(file => Path.GetFileName(file)));
        }

        /// <summary>
        /// Módosítás idejének lekrédezése.
        /// </summary>
        /// <param name="name">A fájl neve.</param>
        /// <returns>Az utolsó módosítás ideje.</returns>
        public async Task<DateTime> GetModifiedTime(String name)
        {
            FileInfo info = new FileInfo(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), name));

            return await Task.Run(() => info.LastWriteTime);
        }
    }
}