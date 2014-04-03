using SelesGames.Instapaper;
using SelesGames.IsoStorage;
using System.Threading.Tasks;

namespace Weave.Services.Instapaper
{
    public class InstapaperAccount2
    {
        public static InstapaperAccount2 Current = new InstapaperAccount2();

        const string ISO_KEY = "instapaper_credentials";

        InstapaperAccount credentials;
        object sync = new object();

        public async Task<InstapaperAccount> GetCredentials()
        {
            if (credentials == null)
            {
                var temp = await new DataContractIsoStorageClient<InstapaperAccount>().GetAsync(ISO_KEY, System.Threading.CancellationToken.None);
                lock (sync)
                {
                    if (credentials == null)
                        credentials = temp;
                }
            }
            return credentials;
        }

        public Task SaveCredentials(InstapaperAccount credentials)
        {
            this.credentials = credentials;
            return new DataContractIsoStorageClient<InstapaperAccount>().SaveAsync(ISO_KEY, credentials, System.Threading.CancellationToken.None);
        }
    }
}
