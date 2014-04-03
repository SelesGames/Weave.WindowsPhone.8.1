using SelesGames.IsoStorage;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.Settings;

namespace Weave.Services
{
    public class DataStorageClient
    {
        const string PERMA_STATE = "permstt";
        const string TOMBSTONE_STATE = "tmbstt";




        #region PermanentState

        public Task<PermanentState> GetPermanentState()
        {
            return CreatePermStateClient()
                .GetOrDefaultAsync(PERMA_STATE, () => new PermanentState());
        }

        public Task Save(PermanentState permanentState)
        {
            return CreatePermStateClient()
                .SaveAsync(PERMA_STATE, permanentState);
        }

        #endregion




        #region TombstoneState

        public Task<TombstoneState> GetTombstoneState()
        {
            return CreateTombstoneStateClient()
                .GetOrDefaultAsync(TOMBSTONE_STATE, () => new TombstoneState());
        }

        public Task Save(TombstoneState tombstoneState)
        {
            return CreateTombstoneStateClient()
                .SaveAsync(TOMBSTONE_STATE, tombstoneState);
        }

        #endregion




        #region Helper methods

        IsoStorageClient<PermanentState> CreatePermStateClient()
        {
            return new JsonIsoStorageClient<PermanentState>();
        }

        IsoStorageClient<TombstoneState> CreateTombstoneStateClient()
        {
            return new JsonIsoStorageClient<TombstoneState>();
        }

        #endregion
    }
}
