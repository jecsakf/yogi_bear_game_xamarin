using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearGame.Persistence
{
    public interface IYogiBearDataAccess
    {
        /// <summary>
        /// Fájl betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        /// <returns>A fájlból beolvasott játéktáblák paraméterei.</returns>
        Task<(YogiBearTable, int)> LoadAsync(string path);

        Task SaveAsync(String path, YogiBearTable table, int time);
    }
}
