using Microsoft.Phone.Shell;

namespace SelesGames.Phone
{
    public static class ShellTileExtensions
    {
        public static bool TryUpdate(this ShellTile tile, ShellTileData data)
        {
            bool updateSuccess = false;

            try
            {
                tile.Update(data);
                updateSuccess = true;
            }
            catch { }

            return updateSuccess;
        }
    }
}
